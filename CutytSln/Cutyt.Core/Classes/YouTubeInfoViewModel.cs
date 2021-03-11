using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Classes
{
    public class YouTubeInfoViewModel
    {
        public string FormatCode { get; set; }

        public string Extension { get; set; }

        public string Resolution { get; set; }

        public string Note { get; set; }

        public string Size { get; set; }

        public string SizeInUI
        {
            get
            {
                var result = string.Empty;
                if (Size?.Length > 0)
                {
                    var firstLetter = Size[0];
                    if (int.TryParse(firstLetter.ToString(), out int dummy))
                    {
                        result = $"({Size})";
                    }
                }

                return result;
            }
        }

        public string TextWithoutCode { get; set; }

        public string DownloadSwitchAudioAndVideo { get; set; }

        /// <summary>
        /// 720p, 1080p etc.
        /// </summary>
        public string VideoResolutionP
        {
            get
            {
                string result = null;

                var parts = ResolutionWidthByHeight?.Split("x", StringSplitOptions.RemoveEmptyEntries);

                if (parts?.Length == 2)
                {
                    result = $"{parts[1]}p";
                }
                return result;
            }
        }

        /// <summary>
        /// 1280x720
        /// </summary>
        public string ResolutionWidthByHeight { get; set; }

        public override string ToString()
        {
            return $"{FormatCode} {TextWithoutCode}";
        }
    }
}
