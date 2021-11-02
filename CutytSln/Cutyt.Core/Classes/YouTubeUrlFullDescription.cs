using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Classes
{
    public class YouTubeUrlFullDescription
    {
        /// <summary>
        /// "Lyubomir Zhechev"
        /// </summary>
        public string Uploader { get; set; }

        /// <summary>
        ///  "https://www.youtube.com/watch?v=g0BJuUHkCbc"
        /// </summary>
        public string Webpage_Url { get; set; }

        /// <summary>
        /// "g0BJuUHkCbc"
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// "watch"
        /// </summary>
        public string Webpage_Url_Basename { get; set; }
        /// <summary>
        /// In seconds. 2890.
        /// </summary>
        public long? Duration { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// "Lyubomir Zhechev"
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// 82
        /// </summary>
        public long? Dislike_Count { get; set; }

        /// <summary>
        /// 25
        /// </summary>
        public double? Fps { get; set; }

        /// <summary>
        /// 133451
        /// </summary>
        public long? View_Count { get; set; }

        /// <summary>
        /// "20210815"
        /// </summary>
        public string Upload_Date { get; set; }

        /// <summary>
        /// Крипто пирамиди 1
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 4.9604869
        /// </summary>
        public double? Average_Rating { get; set; }

        /// <summary>
        /// 8219
        /// </summary>
        public long? Like_Count { get; set; }

        public List<YouTubeFormat> Formats { get; set; }

        public List<YouTubeThumbnail> Thumbnails { get; set; }


    }
}
