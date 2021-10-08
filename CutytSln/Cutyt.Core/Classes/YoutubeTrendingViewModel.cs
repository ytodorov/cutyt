using Cutyt.Core.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Classes
{
    public class YoutubeTrendingViewModel
    {
        public string Id { get; set; }

        public string Url { get; set; }

        public string Name { get; set; }

        public string V { get; set; }

        public long Likes { get; set; }

        public long Dislikes { get; set; }

        public long Views { get; set; }

        public double AverageRating { get; set; }

        public string RegionCode { get; set; }
    }
}
