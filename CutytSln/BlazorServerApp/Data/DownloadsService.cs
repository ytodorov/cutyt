using Cutyt.Core;
using Cutyt.Core.Classes;
using Cutyt.Core.Extensions;
using Cutyt.Core.Rebus.Jobs;
using Cutyt.Core.Rebus.Replies;
using Cutyt.Core.Storage;
using CutytKendoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Web;

namespace BlazorServerApp.Data
{
    public class DownloadsService
    {
        public async Task<List<YoutubeDownloadedFileInfo>> GetDownloadsByIp(string ip)
        {
            string query = $"\"Ip\" = '{ip.Base64StringEncode()}'";
            List<YoutubeDownloadedFileInfo> blobs = await BlobStorageHelper.ListYoutubeDownloadedFileInfoBlobs("media", null, query);

            return blobs;
        }

        public async Task<List<YoutubeDownloadedFileInfo>> GetAllDownloadsForToday()
        {
            string query = $"\"DownloadedOnTicks\" > '{DateTime.UtcNow.Date.Ticks}'";

            List<YoutubeDownloadedFileInfo> blobs = await BlobStorageHelper.ListYoutubeDownloadedFileInfoBlobs("media", null, query);

            return blobs;
        }
    }
}