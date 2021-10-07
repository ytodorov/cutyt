using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Cutyt.Core.Extensions;
using Cutyt.Core.Rebus.Replies;
using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Storage
{
    public class BlobStorageHelper
    {
        public static async Task UploadBlob(string localFilePath, string fileName, string containerName, IDictionary<string, string> metadata, TelemetryClient telemetryClient)
        {
            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=https;AccountName=stcutyt;AccountKey=dL4wNdv+iksMDip5kwx148JepOOV7ajzDQDNyMhinxxYqW6CDYwz+IqCYX2Bb3YIV5gMVo+ABb+iDSaZYg3OTw==;EndpointSuffix=core.windows.net");

            //Create a unique name for the container
            //string containerName = "media";

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(fileName);



            // Upload data from the local file
            await blobClient.UploadAsync(localFilePath, true);

            await AddBlobMetadataAsync(blobClient, metadata, telemetryClient);
        }

        public static async Task<string> GetFirstBlobContent(string containerName, string query)
        {
            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=https;AccountName=stcutyt;AccountKey=dL4wNdv+iksMDip5kwx148JepOOV7ajzDQDNyMhinxxYqW6CDYwz+IqCYX2Bb3YIV5gMVo+ABb+iDSaZYg3OTw==;EndpointSuffix=core.windows.net");

            //Create a unique name for the container
            //string containerName = "media";

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            //List<BlobItem> blobItems = new List<BlobItem>();
            List<YoutubeDownloadedFileInfo> youtubeDownloadedFileInfos = new List<YoutubeDownloadedFileInfo>();

            if (!string.IsNullOrEmpty(query))
            {
                query = $"{query} AND @container = '{containerName}'";
                //blobServiceClient.FindBlobsByTagsAsync(query);
                await foreach (TaggedBlobItem item in blobServiceClient.FindBlobsByTagsAsync(query))
                {
                    // Get a reference to a blob
                    BlobClient blobClient = containerClient.GetBlobClient(item.BlobName);

                    var content = await blobClient.DownloadContentAsync();

                    var stringContent = content.Value.Content.ToString();

                    return stringContent;

                }
            }

            return null;
        }

        public static async Task<List<YoutubeDownloadedFileInfo>> ListYoutubeDownloadedFileInfoBlobs(string containerName, TelemetryClient telemetryClient, string query = null)
        {
            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=https;AccountName=stcutyt;AccountKey=dL4wNdv+iksMDip5kwx148JepOOV7ajzDQDNyMhinxxYqW6CDYwz+IqCYX2Bb3YIV5gMVo+ABb+iDSaZYg3OTw==;EndpointSuffix=core.windows.net");

            //Create a unique name for the container
            //string containerName = "media";

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            //List<BlobItem> blobItems = new List<BlobItem>();
            List<YoutubeDownloadedFileInfo> youtubeDownloadedFileInfos = new List<YoutubeDownloadedFileInfo>();

            if (!string.IsNullOrEmpty(query))
            {
                query = $"{query} AND @container = '{containerName}'";
                //blobServiceClient.FindBlobsByTagsAsync(query);
                await foreach (TaggedBlobItem item in blobServiceClient.FindBlobsByTagsAsync(query))
                {
                    // Get a reference to a blob
                    BlobClient blobClient = containerClient.GetBlobClient(item.BlobName);
                    var metadata = await ReadBlobMetadataAsync(blobClient, telemetryClient);

                    YoutubeDownloadedFileInfo youtubeDownloadedFileInfo = new YoutubeDownloadedFileInfo();

                    metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.Start), out string start);
                    metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.End), out string end);
                    metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.FileOnDiskSize), out string fileOnDiskSize);
                    metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.DisplayName), out string displayName);
                    metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.Url), out string url);
                    metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.Ip), out string Ip);
                    metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.Id), out string Id);
                    metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.DownloadedOn), out string downloadedOn);
                    metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.FileOnDiskExtension), out string fileOnDiskExtension);

                    //youtubeDownloadedFileInfo.DownloadedOn = blobItem.Properties.CreatedOn.GetValueOrDefault().UtcDateTime;
                    youtubeDownloadedFileInfo.Start = start.Base64StringDecode();
                    youtubeDownloadedFileInfo.End = end.Base64StringDecode();
                    youtubeDownloadedFileInfo.DisplayName = displayName.Base64StringDecode();
                    youtubeDownloadedFileInfo.Url = url.Base64StringDecode();
                    if (long.TryParse(fileOnDiskSize.Base64StringDecode(), out long longFileOnDiskSize))
                    {
                        youtubeDownloadedFileInfo.FileOnDiskSize = longFileOnDiskSize;
                    }

                    youtubeDownloadedFileInfo.Ip = Ip.Base64StringDecode();

                    youtubeDownloadedFileInfo.Id = Id.Base64StringDecode();
                    youtubeDownloadedFileInfo.FileOnDiskExtension = fileOnDiskExtension.Base64StringDecode();

                    if (DateTime.TryParse(downloadedOn.Base64StringDecode(), out DateTime downloadedOnDate))
                    {
                        youtubeDownloadedFileInfo.DownloadedOn = downloadedOnDate;
                    }


                    youtubeDownloadedFileInfos.Add(youtubeDownloadedFileInfo);

                    youtubeDownloadedFileInfos = youtubeDownloadedFileInfos.OrderByDescending(d => d.DownloadedOn).ToList();
                }
            }



            else
            {
                await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(BlobTraits.Metadata))
                {
                    YoutubeDownloadedFileInfo youtubeDownloadedFileInfo = new YoutubeDownloadedFileInfo();

                    blobItem.Metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.Start), out string start);
                    blobItem.Metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.End), out string end);
                    blobItem.Metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.FileOnDiskSize), out string fileOnDiskSize);
                    blobItem.Metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.DisplayName), out string displayName);
                    blobItem.Metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.Url), out string url);
                    blobItem.Metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.Ip), out string Ip);
                    blobItem.Metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.Id), out string Id);
                    blobItem.Metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.DownloadedOn), out string downloadedOn);

                    blobItem.Metadata.TryGetValue(nameof(YoutubeDownloadedFileInfo.FileOnDiskExtension), out string fileOnDiskExtension);

                    youtubeDownloadedFileInfo.DownloadedOn = blobItem.Properties.CreatedOn.GetValueOrDefault().UtcDateTime;
                    youtubeDownloadedFileInfo.Start = start.Base64StringDecode();
                    youtubeDownloadedFileInfo.End = end.Base64StringDecode();
                    youtubeDownloadedFileInfo.DisplayName = displayName.Base64StringDecode();
                    youtubeDownloadedFileInfo.Url = url.Base64StringDecode();
                    if (long.TryParse(fileOnDiskSize.Base64StringDecode(), out long longFileOnDiskSize))
                    {
                        youtubeDownloadedFileInfo.FileOnDiskSize = longFileOnDiskSize;
                    }

                    youtubeDownloadedFileInfo.Ip = Ip.Base64StringDecode();

                    youtubeDownloadedFileInfo.Id = Id.Base64StringDecode();
                    youtubeDownloadedFileInfo.FileOnDiskExtension = fileOnDiskExtension.Base64StringDecode();

                    if (DateTime.TryParse(downloadedOn, out DateTime downloadedOnDate))
                    {
                        youtubeDownloadedFileInfo.DownloadedOn = downloadedOnDate;
                    }

                    youtubeDownloadedFileInfos.Add(youtubeDownloadedFileInfo);

                    youtubeDownloadedFileInfos = youtubeDownloadedFileInfos.OrderByDescending(d => d.DownloadedOn).ToList();
                }
            }

            return youtubeDownloadedFileInfos;
        }

        // https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-properties-metadata?tabs=dotnet

        public static async Task AddBlobMetadataAsync(BlobClient blob, IDictionary<string, string> metadata, TelemetryClient telemetryClient)
        {
            try
            {
                // Set the blob's metadata.
                await blob.SetMetadataAsync(metadata);

                await blob.SetTagsAsync(metadata);

            }
            catch (RequestFailedException e)
            {
                telemetryClient.TrackException(e);
            }
        }

        public static async Task<IDictionary<string, string>> ReadBlobMetadataAsync(BlobClient blob, TelemetryClient telemetryClient)
        {
            try
            {
                // Get the blob's properties and metadata.
                BlobProperties properties = await blob.GetPropertiesAsync();

                return properties.Metadata;

            }
            catch (RequestFailedException e)
            {
                telemetryClient.TrackException(e);
            }

            return new Dictionary<string, string>();

        }

        private static async Task GetBlobPropertiesAsync(BlobClient blob)
        {
            try
            {
                // Get the blob properties
                BlobProperties properties = await blob.GetPropertiesAsync();

                // Display some of the blob's property values
                Console.WriteLine($" ContentLanguage: {properties.ContentLanguage}");
                Console.WriteLine($" ContentType: {properties.ContentType}");
                Console.WriteLine($" CreatedOn: {properties.CreatedOn}");
                Console.WriteLine($" LastModified: {properties.LastModified}");
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine($"HTTP error code {e.Status}: {e.ErrorCode}");
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }

        public static async Task SetBlobPropertiesAsync(BlobClient blob)
        {
            Console.WriteLine("Setting blob properties...");

            try
            {
                // Get the existing properties
                BlobProperties properties = await blob.GetPropertiesAsync();

                BlobHttpHeaders headers = new BlobHttpHeaders
                {
                    // Set the MIME ContentType every time the properties 
                    // are updated or the field will be cleared
                    ContentType = "text/plain",
                    ContentLanguage = "en-us",

                    // Populate remaining headers with 
                    // the pre-existing properties
                    CacheControl = properties.CacheControl,
                    ContentDisposition = properties.ContentDisposition,
                    ContentEncoding = properties.ContentEncoding,
                    ContentHash = properties.ContentHash
                };

                // Set the blob's properties.
                await blob.SetHttpHeadersAsync(headers);
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine($"HTTP error code {e.Status}: {e.ErrorCode}");
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }
    }
}
