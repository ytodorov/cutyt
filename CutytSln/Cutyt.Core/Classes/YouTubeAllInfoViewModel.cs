using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Classes
{
    public class YouTubeAllInfoViewModel
    {
        public List<YouTubeInfoViewModel> Infos { get; set; }

        public int DurationInTensOfSeconds => DurationInSeconds * 10;

        public int DurationInSeconds { get; set; }
    }
}
