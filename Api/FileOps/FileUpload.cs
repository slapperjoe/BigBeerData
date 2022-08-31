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
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System.Text;

namespace Api.FileOps
{
    public class FileUpload
    {
        [FunctionName("FileUpload")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            var data = await req.Content.ReadAsMultipartAsync();

            foreach (var file in data.Contents) {

                using (MemoryStream ms = new MemoryStream())
                {
                    string storageConnectionString = System.Environment.GetEnvironmentVariable("BigBeerStorageAccount");
                    CloudStorageAccount storageAccount;
                    if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
                    {
                        // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                        CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                        CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("bigbeercontainer");
                        await cloudBlobContainer.CreateIfNotExistsAsync();

                        CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(file.Headers.ContentDisposition.FileName);
                        cloudBlockBlob.Properties.ContentType = file.Headers.ContentType.ToString();
                           
                        try
                        {
                            await cloudBlockBlob.UploadFromStreamAsync(await file.ReadAsStreamAsync());
                        }
                        catch (Exception ex) {
                            log.Error(ex.Message);
                            return new HttpResponseMessage(HttpStatusCode.BadRequest)
                            {
                                Content = new StringContent(ex.Message),
                            };
                        }
                    }
                }

                //System.Drawing.Image img = System.Drawing.Image.FromFile(file.Headers.ContentDisposition.FileName);
                //var fileStream = await file.ReadAsStreamAsync();
                //img.Save(fileStream, System.Drawing.Imaging.ImageFormat.Png);
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Uploaded to Storage Blob"),
            };
        }
    }
}

