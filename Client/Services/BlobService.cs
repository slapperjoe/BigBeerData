using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Client.DTOs;

namespace Client.Services
{
    public class BlobService
    {
        private readonly string _blobConnectionString;
        private const string _blobContainer = "bigbeercontainer";

        public BlobService(IConfiguration configuration)
        {
            _blobConnectionString = configuration.GetConnectionString("BlobStore") ?? String.Empty;
        }

        public async Task<List<BlobDto>> GetBlobFiles()
        {
            var blobs = new List<BlobDto>();
            var container = new BlobContainerClient(_blobConnectionString, _blobContainer);

            await foreach (var blob in container.GetBlobsAsync())
            {
                var blobDto = new BlobDto()
                {
                    Name = blob.Name,
                    FileUrl = container.Uri.AbsoluteUri + "/" + blob.Name,
                    ContentType = blob.Properties.ContentType
                };
                blobs.Add(blobDto);
            }
            return blobs;
        }

        public async Task<ContentDto> GetBlobFile(string name)
        {
            var container = new BlobContainerClient(_blobConnectionString, _blobContainer);
            var blob = container.GetBlobClient(name);

            if (await blob.ExistsAsync())
            {
                var a = await blob.DownloadAsync();
                var contentDto = new ContentDto()
                {
                    Content = a.Value.Content,
                    ContentType = a.Value.ContentType,
                    Name = name
                };

                return contentDto;
            }

            throw new FileLoadException();
        }

        public async Task<Azure.Response<BlobContentInfo>> UploadBlobAsync(string fileName, Stream stream)
        {
            var container = new BlobContainerClient(_blobConnectionString, _blobContainer);
            var result = await container.UploadBlobAsync(fileName, stream);
            return result;
        }

    }
}
