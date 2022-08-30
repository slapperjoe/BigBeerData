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

namespace Api.FileOps
{
    public class FileUpload
    {
        [FunctionName("FileUpload")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string name;

            // Get request body
            dynamic data = await req.Content.ReadAsFormDataAsync();

            // Set name to query string or body data
            name = data?.path;

            //Image img = Image.FromFile(name);
            //byte[] arr;
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            //    arr = ms.GetBuffer();

            //    string storageConnectionString = System.Environment.GetEnvironmentVariable("StorageConnection");
            //    CloudStorageAccount storageAccount;
            //    if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            //    {
            //        // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
            //        CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            //        CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("ContainerName");
            //        await cloudBlobContainer.CreateIfNotExistsAsync();

            //        CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference("Image1.jpeg");
            //        cloudBlockBlob.Properties.ContentType = "image\\jpeg";

            //        ms.Seek(0, SeekOrigin.Begin);
            //        cloudBlockBlob.UploadFromStream(ms);
            //    }
            //}


            return name == null
            ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a Path  in the request body")
            : req.CreateResponse(HttpStatusCode.OK, "Uploaded to Storage Blob");
        }
    }
}

