
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.WebUtilities;
using Api.Helpers;
using Microsoft.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Azure.NotificationHubs;
using System.Xml;
using SixLabors.ImageSharp.Formats.Png;
using Image = SixLabors.ImageSharp.Image;
using System.Text.Json;
using BigBeerData.Shared.DTOs;
using BigBeerData.Shared;

namespace Api.FileOps
{
	public class FileUpload
	{
		private readonly ILogger _logger;
		private const string ContentTypeHeader = "Content-Type";
		private const string TextPlainUtf8 = "text/plain; charset=utf-8";

		const string MD5Key = "ParsedMD5";

		public FileUpload(ILoggerFactory loggerFactory)
		{
			_logger = loggerFactory.CreateLogger<FileUpload>();
		}

		[Function("FileUpload")]
		public async Task<HttpResponseData> RunFileUpload([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
		{
			if (!req.Headers.TryGetValues(ContentTypeHeader, out var contentTypeValues))
			{
				var bad = req.CreateResponse(HttpStatusCode.BadRequest);
				bad.Headers.Add(ContentTypeHeader, TextPlainUtf8);
				await bad.WriteStringAsync("Missing Content-Type header");
				return bad;
			}

			var contentType = contentTypeValues.FirstOrDefault();
			if (string.IsNullOrWhiteSpace(contentType))
			{
				var bad = req.CreateResponse(HttpStatusCode.BadRequest);
				bad.Headers.Add(ContentTypeHeader, TextPlainUtf8);
				await bad.WriteStringAsync("Invalid Content-Type header");
				return bad;
			}

			var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(contentType), (int)req.Body.Length);
			var reader = new MultipartReader(boundary, req.Body);
			var data = await reader.ReadNextSectionAsync();

			while (data != null)
			{
				var file = data.AsFileSection();
				if (file == null || string.IsNullOrWhiteSpace(file.FileName))
				{
					data = await reader.ReadNextSectionAsync();
					continue;
				}
				if (file.FileStream == null)
				{
					data = await reader.ReadNextSectionAsync();
					continue;
				}

				var baseName = Path.GetFileNameWithoutExtension(file.FileName);
				var pngName = string.IsNullOrWhiteSpace(baseName) ? "upload.png" : baseName + ".png";
				using (MemoryStream ms = new())
				{
					string storageConnectionString = System.Environment.GetEnvironmentVariable("BigBeerStorageAccount") ?? String.Empty;
					try
					{
						var blobServiceClient = new BlobServiceClient(storageConnectionString);

						var cloudBlobContainer = blobServiceClient.GetBlobContainerClient("bigbeercontainer");
						await cloudBlobContainer.CreateIfNotExistsAsync();

						BlockBlobClient cloudBlockBlob = cloudBlobContainer.GetBlockBlobClient(pngName);

						await file.FileStream.CopyToAsync(ms);
						ms.Position = 0;
						await using MemoryStream ms2 = new();
						using (Image img = Image.Load(ms))
						{
							img.Save(ms2, new PngEncoder());
						}

						ms2.Position = 0;
						await cloudBlockBlob.UploadAsync(ms2);

						await this.GenerateNotification(ms2, cloudBlockBlob, pngName);

						await cloudBlockBlob.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = "image/png" });
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, ex.Message);
						var errorresponse = req.CreateResponse(HttpStatusCode.InternalServerError);
						errorresponse.Headers.Add(ContentTypeHeader, TextPlainUtf8);
						await errorresponse.WriteStringAsync(ex.Message);
						return errorresponse;
					}
				}
				data = await reader.ReadNextSectionAsync();
			}

			var response = req.CreateResponse(HttpStatusCode.OK);
			response.Headers.Add(ContentTypeHeader, TextPlainUtf8);

			await response.WriteStringAsync("Uploaded to Storage Blob");
			return response;
		}

		[Function("DataUpload")]
		public async Task<HttpResponseData> RunDataUpload([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
		{
			var jsonString = await new StreamReader(req.Body).ReadToEndAsync();

			var beerDto = JsonSerializer.Deserialize<BeerDTO>(jsonString, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});
			if (beerDto == null)
			{
				var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
				badResponse.Headers.Add(ContentTypeHeader, TextPlainUtf8);
				await badResponse.WriteStringAsync("Invalid beer payload");
				return badResponse;
			}

			string storageConnectionString = System.Environment.GetEnvironmentVariable("BigBeerStorageAccount") ?? String.Empty;
			try
			{
				string fileName = beerDto.tapNo + ".json";
				var blobServiceClient = new BlobServiceClient(storageConnectionString);

				var cloudBlobContainer = blobServiceClient.GetBlobContainerClient("bigbeercontainer");
				await cloudBlobContainer.CreateIfNotExistsAsync();

				BlockBlobClient cloudBlockBlob = cloudBlobContainer.GetBlockBlobClient(fileName);

				req.Body.Position = 0;
				await cloudBlockBlob.UploadAsync(req.Body);
				await cloudBlockBlob.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = "application/json" });

				req.Body.Position = 0;


				await SendNotification(cloudBlockBlob, fileName);
			}
			catch (Exception ex)
			{			
				var errorresponse = req.CreateResponse(HttpStatusCode.InternalServerError);
				errorresponse.Headers.Add(ContentTypeHeader, TextPlainUtf8);
				await errorresponse.WriteStringAsync(ex.Message);
				await errorresponse.WriteStringAsync(ex.StackTrace ?? "No stack trace");
				return errorresponse;
			}

			var response = req.CreateResponse(HttpStatusCode.OK);
			response.Headers.Add(ContentTypeHeader, TextPlainUtf8);

			await response.WriteStringAsync("Uploaded to Storage Blob");
			return response;
		}

		protected async Task GenerateNotification(MemoryStream ms, BlockBlobClient blob, string name)
		{
			ms.Position = 0;
			byte[] md5 = MD5.HashData(ms.ToArray());

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
				await SendNotification(blob, name);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error checking upload");
			}

		}

		protected static async Task<NotificationOutcome> SendNotification(BlockBlobClient blob, string name) {
			NotificationHubClient clientHub = NotificationHubClient
							.CreateClientFromConnectionString("Endpoint=sb://BigBeerHub.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=05Q8T0gXQ2QxcP25woaXMtbEqAQP8NOGolQO0+FMUlU=", "TappAppUpdates");

			XmlDocument beerToast = new();
			beerToast.Load("./datastore/BeerUpdate.xml");

			string tap = name.Substring(0, name.IndexOf('.'));

			var tapAttr = beerToast.SelectSingleNode("toast/@tap-number") as XmlAttribute;
			if (tapAttr != null)
			{
				tapAttr.Value = tap;
			}
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
			return await clientHub.SendWindowsNativeNotificationAsync(beerToast.InnerXml);
		}
	}
}
