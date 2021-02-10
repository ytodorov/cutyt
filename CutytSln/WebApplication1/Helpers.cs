using Azure;
using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1
{
    public static class Helpers
    {
        public static void UploadFileInAzureFileShare(string localFilePath, IHostEnvironment hostingEnvironment)
        {
            // Get a connection string to our Azure Storage account.  You can
            // obtain your connection string from the Azure Portal (click
            // Access Keys under Settings in the Portal Storage account blade)
            // or using the Azure CLI with:
            //
            //     az storage account show-connection-string --name <account_name> --resource-group <resource_group>
            //
            // And you can provide the connection string to your application
            // using an environment variable.            
            string connectionString = "BlobEndpoint=https://stcutyt.blob.core.windows.net/;QueueEndpoint=https://stcutyt.queue.core.windows.net/;FileEndpoint=https://stcutyt.file.core.windows.net/;TableEndpoint=https://stcutyt.table.core.windows.net/;SharedAccessSignature=sv=2019-12-12&ss=f&srt=sco&sp=rwlc&se=3021-02-10T16:48:10Z&st=2020-02-10T08:48:10Z&sip=77.70.29.69&spr=https,http&sig=MsFKUyMANeKJVKvm0VROz6yU%2FumN9cHVbeQNktd7qq0%3D";

            if (!hostingEnvironment.IsDevelopment())
            {
                connectionString = "BlobEndpoint=https://stcutyt.blob.core.windows.net/;QueueEndpoint=https://stcutyt.queue.core.windows.net/;FileEndpoint=https://stcutyt.file.core.windows.net/;TableEndpoint=https://stcutyt.table.core.windows.net/;SharedAccessSignature=sv=2019-12-12&ss=f&srt=sco&sp=rwlc&se=5021-02-10T17:09:05Z&st=2020-02-10T09:09:05Z&sip=51.136.25.67&spr=https,http&sig=C9WDKKQR685I7XGD9Poj0yu0%2FdXH4ELCm9pZSP3GVkQ%3D";
            }

            // Name of the share, directory, and file we'll create
            string shareName = "cutyt";
            string dirName = "";
            //string fileName = "sample-file";

            // Path to the local file to upload
            //string localFilePath = @"<path_to_local_file>";
            string fileName = Path.GetFileName(localFilePath);

            // Get a reference to a share and then create it
            ShareClient share = new ShareClient(connectionString, shareName);
            //share.Create();

            // Get a reference to a directory and create it
            ShareDirectoryClient directory = share.GetDirectoryClient(dirName);
            //directory.Create();

            // Get a reference to a file and upload it
            ShareFileClient file = directory.GetFileClient(fileName);
            using (FileStream stream = File.OpenRead(localFilePath))
            {
                file.Create(stream.Length);
                file.UploadRange(
                    new HttpRange(0, stream.Length),
                    stream);
            }
        }
    }
}
