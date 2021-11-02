using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cutyt.Core.Classes
{
    public class YouTubeThumbnail
    {
        public string Url { get; set; }

        [JsonPropertyName("_test_url")]
        public bool? TestUrl { get; set; }

        public double? Preference { get; set; }

        public string Id { get; set; }
    }
}
