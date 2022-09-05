using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;
using Microsoft.Azure.Storage.Blob;

namespace Api.FileOps
{

    
    public class FileChanged
    {

        const string MD5Key = "ParsedMD5";

        [FunctionName("FileChanged")]
        public async Task Run(
            [BlobTrigger("bigbeercontainer/{name}", Connection = "BigBeerStorageAccount")] ICloudBlob blob,
            //[ServiceBus("process", Connection = "sbcss", EntityType = EntityType.Queue)] ICollector queueCollector,
            string name,
            Uri uri, // blob primary location
            IDictionary<string, string> metaData, // user-defined blob metadata
            BlobProperties properties, // blob system properties, e.g. LastModified
            ILogger log)
        {
            if (metaData != null && metaData.ContainsKey(MD5Key) && metaData[MD5Key] == properties.ContentMD5)
            {
                // old content
            } else
            {
                blob.Metadata[MD5Key] = properties.ContentMD5;
                await blob.SetMetadataAsync();
            }
        }
    }
}

