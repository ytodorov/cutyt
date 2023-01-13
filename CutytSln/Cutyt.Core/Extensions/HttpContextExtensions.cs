using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAParser;

namespace Cutyt.Core.Extensions
{
    public static class HttpContextExtensions
    {
        public static bool IsMobileBrowser(this HttpContext httpContext)
        {
            bool result = true;

            if (httpContext?.Request?.Headers?.ContainsKey("User-Agent") == true)
            {
                string uaString = httpContext.Request.Headers["User-Agent"].ToString();

                // get a parser with the embedded regex patterns
                Parser uaParser = Parser.GetDefault();

                // get a parser using externally supplied yaml definitions
                // var uaParser = Parser.FromYaml(yamlString);

                ClientInfo c = uaParser.Parse(uaString);

                if (c?.UA?.Family?.Contains("Mobile", StringComparison.InvariantCultureIgnoreCase) != true)
                {
                    result = false;
                }
            }

            return result;

        }
    }
}
