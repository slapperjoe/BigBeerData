using System.Collections.Generic;
using System.Net;

using System.Net.Http;
using System.Reflection.PortableExecutable;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.WebUtilities;
using Azure.Core;
using Api.Helpers;
using Microsoft.Net.Http.Headers;

namespace Api.FileOps
{
    public class FileUpload
    {
        private readonly ILogger _logger;

        public FileUpload(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FileUpload>();
        }

        [Function("FileUpload")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(req.Headers.GetValues("Content-Type").FirstOrDefault()), (int)req.Body.Length);

            var reader = new MultipartReader(boundary, req.Body);

            var data = await reader.ReadNextSectionAsync();

            while (data != null)
            {
                var file = data.AsFileSection();
                using (MemoryStream ms = new())
                {
                    string storageConnectionString = System.Environment.GetEnvironmentVariable("BigBeerStorageAccount") ?? String.Empty;
                    try
                    {
                        var blobServiceClient = new BlobServiceClient(storageConnectionString);

                        var cloudBlobContainer = blobServiceClient.GetBlobContainerClient("bigbeercontainer");
                        await cloudBlobContainer.CreateIfNotExistsAsync();

                        BlockBlobClient cloudBlockBlob = cloudBlobContainer.GetBlockBlobClient(file.FileName);

                        await file.FileStream.CopyToAsync(ms);
                        ms.Position = 0;

                        await cloudBlockBlob.UploadAsync(ms);

                        var props = 

                        await cloudBlockBlob.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = data.ContentType});
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        var errorresponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                        errorresponse.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                        errorresponse.WriteString(ex.Message);
                        return errorresponse;
                    }
                }
                data = await reader.ReadNextSectionAsync();
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Uploaded to Storage Blob");

            return response;
        }
    }
}
