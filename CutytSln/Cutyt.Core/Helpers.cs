using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core
{
    public static class Helpers
    {
        public static string GetFullUrlFromYouTube(string url, HttpClient httpClient)
        {
            if (url?.ToLowerInvariant()?.Contains("youtu.be", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                //var response = httpClient.GetAsync(url).Result;
                //var fullUrl = response?.RequestMessage?.RequestUri?.ToString();

                // https://youtu.be/H20Nr4CXa4k

                Uri uri = new Uri(url);

                var v = uri.Segments.FirstOrDefault(f => f.Length > 1);

                var fullUrl = $"https://www.youtube.com/watch?v={v}";

                // https://consent.youtube.com/ml?continue=https://www.youtube.com/watch?v=_xtloJqfIrs&feature=youtu.be
                //fullUrl = fullUrl.Replace("https://consent.youtube.com/ml?continue=", string.Empty);
                
                return fullUrl;
            }
            else
            {
                return url;
            }
        }
    }
}
