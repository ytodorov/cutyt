using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Classes
{
    public class YouTubeAllInfoViewModel
    {
        public List<YouTubeFormat> Formats { get; set; }

        public long DurationInTensOfSeconds => DurationInSeconds * 10;

        public long DurationInSeconds { get; set; }

        public string Title { get; set; }
    }
}
