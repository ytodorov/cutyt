using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Extensions
{
    public static class HttpRequestExtensions
    {
        public static async Task<string> GetRawBodyAsync(this HttpRequest request, Encoding encoding = null)
        {
                if (!request.Body.CanSeek)
                {
                // We only do this if the stream isn't *already* seekable,
                // as EnableBuffering will create a new stream instance
                // each time it's called
                /*
                 * System.IO.IOException in AZURE if called without arguments.
Exception message	There is not enough space on the disk. : 'D:\local\Temp\ASPNETCORE_4a57b0d5-915b-4857-9d7b-164708b38d2f.tmp' 
                 */
                request.EnableBuffering(int.MaxValue, int.MaxValue);
                }

                request.Body.Position = 0;

                var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);

                var body = await reader.ReadToEndAsync().ConfigureAwait(false);

                request.Body.Position = 0;

                return body;
        }
    }
}
