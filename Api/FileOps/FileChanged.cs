using System;
using System.IO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Security.Cryptography;
using Azure.Storage.Blobs.Specialized;
using System.Text;

namespace Api.FileOps
{
    public class FileChanged
    {
        const string MD5Key = "ParsedMD5";
        private readonly ILogger _logger;

        public FileChanged(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FileChanged>();
        }

        [Function("FileChanged")]
        public async Task Run([BlobTrigger("bigbeercontainer/{name}", Connection = "BigBeerStorageAccount")]
            ReadOnlyMemory<byte> blob,
            //[ServiceBus("process", Connection = "sbcss", EntityType = EntityType.Queue)] ICollector queueCollector,
            string name,
            Uri uri, // blob primary location
            BlobProperties properties,
            IDictionary<string, string> metaData) // user-defined blob metadata
        {
            byte[] md5 = MD5.HashData(blob.ToArray());
            string sMD5 = Encoding.UTF8.GetString(md5);
            BlobClient blobClient = new(uri);

            string storageConnectionString = System.Environment.GetEnvironmentVariable("BigBeerStorageAccount") ?? String.Empty;
            try
            {
                var blobServiceClient = new BlobServiceClient(storageConnectionString);
                var cloudBlobContainer = blobServiceClient.GetBlobContainerClient("bigbeercontainer");
                await cloudBlobContainer.CreateIfNotExistsAsync();

                BlockBlobClient cloudBlockBlob = cloudBlobContainer.GetBlockBlobClient(name);

                //_logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {path}");
                if (metaData != null && metaData.ContainsKey(MD5Key) && metaData[MD5Key] == sMD5)
                {
                    // old content
                }
                else
                {

                    var metadata = new Dictionary<string, string>
                    {
                        { MD5Key, sMD5 ?? String.Empty }
                    };
                    await cloudBlockBlob.SetMetadataAsync(metadata);
                }
            } catch (Exception e)
            {
                _logger.LogError(e, "Error checking upload");
            }

               
        }
    }
}
