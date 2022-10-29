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
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Azure.NotificationHubs;
using System.Xml;
using ImageMagick;

namespace Api.FileOps
{
	public class FileUpload
	{
		private readonly ILogger _logger;

		const string MD5Key = "ParsedMD5";

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

						using (var bmp = new MagickImage(ms))
						{
							using (var memStream = new MemoryStream())
							{
								bmp.Format = MagickFormat.Png;
								bmp.Write(memStream);
								await cloudBlockBlob.UploadAsync(memStream);

								await this.GenerateNotification(memStream, cloudBlockBlob, file.FileName);
							}
						}						

						var props = await cloudBlockBlob.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = data.ContentType });
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

		protected async Task GenerateNotification(MemoryStream ms, BlockBlobClient blob, string name)
		{
			ms.Position = 0;
			byte[] md5 = MD5.HashData(ms.ToArray());
			//string sMD5 = Encoding.UTF8.GetString(md5);

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < md5.Length; i++)
			{
				sb.Append(md5[i].ToString("x2"));
			}
			string sMD5 = sb.ToString();

			try
			{

				var metadata = new Dictionary<string, string>
										{
												{ MD5Key, sMD5 ?? String.Empty }
										};
				await blob.SetMetadataAsync(metadata);

				NotificationHubClient clientHub = NotificationHubClient
						.CreateClientFromConnectionString("Endpoint=sb://BigBeerHub.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=05Q8T0gXQ2QxcP25woaXMtbEqAQP8NOGolQO0+FMUlU=", "TappAppUpdates");

				XmlDocument beerToast = new();
				beerToast.Load("./datastore/BeerUpdate.xml");

				string tap = name.Substring(0, name.IndexOf('.'));

				XmlNode? textNode = beerToast.SelectSingleNode("/toast/visual/binding/text");
				if (textNode != null)
				{
					textNode.InnerText = $"A beer has been updated on tap #{tap}";
				}
				XmlNode? imageNode = beerToast.SelectSingleNode("/toast/visual/binding/image");
				if (imageNode != null)
				{
					((XmlElement)imageNode).SetAttribute("src", $"https://cs1c08048ede1dax4ddbx836.blob.core.windows.net/bigbeercontainer/{tap}.png?m={DateTime.Now.ToBinary()}");
				}
				var notificationResult = await clientHub.SendWindowsNativeNotificationAsync(beerToast.InnerXml);


			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error checking upload");
			}

		}
	}
}
