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
                var response = httpClient.GetAsync(url).Result;
                var fullUrl = response?.RequestMessage?.RequestUri?.ToString();
                return fullUrl;
            }
            else
            {
                return url;
            }
        }
    }
}
