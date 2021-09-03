using Cutyt.Core.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Rebus.Replies
{
    public class YoutubeDownloadLinkReply
    {
        public string Id { get; set; }

        public string Url { get; set; }

        public string Name { get; set; }

        public string V { get; set; }

        public string FileName { get; set; }

        public string FileNameWithVideoFormatCode { get; set; }

        public string FormatCode { get; set; }

        public string DisplayName { get; set; }

        public string Start { get; set; }

        public string End { get; set; }

        public string Ip { get; set; }

        public string FileOnDiskNameWithExtension { get; set; }

        public string FileOnDiskExtension { get; set; }

        public string FileOnDiskNameWithoutExtension { get; set; }

        public DateTime DownloadedOn { get; set; }

        public long FileOnDiskSize { get; set; }

        public double FileOnDiskSizeInMegabytes
        {
            get
            {
                var size = Math.Round((double)FileOnDiskSize / 1014 / 1024, 2);
                return size;
            }
        }
    }
}
