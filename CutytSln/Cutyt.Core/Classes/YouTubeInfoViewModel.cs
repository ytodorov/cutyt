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

        public string TextWithoutCode { get; set; }
    }
}
