using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Rebus.Jobs
{
    public class GetYoutubeDownloadLinkJob
    {
        public string Url { get; set; }

		public string MyProperty { get; set; }

		public string OutputFileName { get; set; }

		public string SelectedOption { get; set; }

		public bool? ShouldTrim { get; set; }

		public double Start { get; set; }

		public double End { get; set; }

		public string V { get; set; }

        public string Ip { get; set; }

        /*
         
         var encodedUrl = $"{serverAddressOfServices}home/exec?args=-f {HttpUtility.UrlEncode(selectedOption)}" +
                $" --no-part \"{url}\" --output \"{outputFileName}.%(ext)s\" -k -v&ytUrl={url}&V={v}&selectedOption={HttpUtility.UrlEncode(selectedOption)}&shouldTrim={shouldTrim.GetValueOrDefault()}" +
                $"&start={start}&end={end}";

         */
    }
}
