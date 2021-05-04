using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Classes
{
    public class YouTubeInfoResult
    {
        public string V { get; set; }

        public string VimeoId { get; set; }

        public double Start { get; set; }

        public double End { get; set; }

        public List<YouTubeInfoViewModel> Infos { get; set; }

        public YouTubeInfoViewModel LastInfo
        {
            get
            {
                var lastInfo = Infos.LastOrDefault();
                return lastInfo;
            }
        }
    }
}
