using System;
using System.IO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Security.Cryptography;
using Azure.Storage.Blobs.Specialized;
using System.Text;

using Microsoft.Azure.NotificationHubs;
using System.Diagnostics;
using System.Xml;

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

		//[Function("FileChanged")]
		//public async Task Run([BlobTrigger("bigbeercontainer/{name}", Connection = "BigBeerStorageAccount")]
		//				ReadOnlyMemory<byte> blob,
		//		//[ServiceBus("process", Connection = "sbcss", EntityType = EntityType.Queue)] ICollector queueCollector,
		//		string name,
		//		Uri uri, // blob primary location
		//		BlobProperties properties,
		//		IDictionary<string, string> metaData) // user-defined blob metadata
		//{
		//	byte[] md5 = MD5.HashData(blob.ToArray());
		//	//string sMD5 = Encoding.UTF8.GetString(md5);

		//	StringBuilder sb = new StringBuilder();
		//	for (int i = 0; i < md5.Length; i++)
		//	{
		//		sb.Append(md5[i].ToString("x2"));
		//	}
		//	string sMD5 = sb.ToString();

		//	string storageConnectionString = System.Environment.GetEnvironmentVariable("BigBeerStorageAccount")
		//			?? String.Empty;
		//	try
		//	{
		//		var blobServiceClient = new BlobServiceClient(storageConnectionString);
		//		var cloudBlobContainer = blobServiceClient.GetBlobContainerClient("bigbeercontainer");
		//		await cloudBlobContainer.CreateIfNotExistsAsync();

		//		BlockBlobClient cloudBlockBlob = cloudBlobContainer.GetBlockBlobClient(name);

		//		if (metaData != null && metaData.ContainsKey(MD5Key) && metaData[MD5Key] == sMD5)
		//		{
		//			// old content
		//		}
		//		else
		//		{

		//			var metadata = new Dictionary<string, string>
		//								{
		//										{ MD5Key, sMD5 ?? String.Empty }
		//								};
		//			await cloudBlockBlob.SetMetadataAsync(metadata);

		//			NotificationHubClient clientHub = NotificationHubClient
		//					.CreateClientFromConnectionString("Endpoint=sb://BigBeerHub.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=05Q8T0gXQ2QxcP25woaXMtbEqAQP8NOGolQO0+FMUlU=", "TappAppUpdates");

		//			XmlDocument beerToast = new();
		//			beerToast.Load("./datastore/BeerUpdate.xml");

		//			string tap = name.Substring(0, name.IndexOf('.'));

		//			XmlNode? textNode = beerToast.SelectSingleNode("/toast/visual/binding/text");
		//			if (textNode != null)
		//			{
		//				textNode.InnerText = $"A beer has been updated on tap #{tap}";
		//			}
		//			XmlNode? imageNode = beerToast.SelectSingleNode("/toast/visual/binding/image");
		//			if (imageNode != null)
		//			{
		//				((XmlElement)imageNode).SetAttribute("src", $"https://cs1c08048ede1dax4ddbx836.blob.core.windows.net/bigbeercontainer/{tap}.png?m={DateTime.Now.ToBinary()}");
		//			}
		//			var notificationResult = await clientHub.SendWindowsNativeNotificationAsync(beerToast.InnerXml);

		//		}
		//	}
		//	catch (Exception e)
		//	{
		//		_logger.LogError(e, "Error checking upload");
		//	}


		//}
	}
}
