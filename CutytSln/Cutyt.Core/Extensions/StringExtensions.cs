using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Base64StringEncode(this string originalString)
        {
            if (string.IsNullOrEmpty(originalString))
            {
                return null;
            }

            var bytes = Encoding.UTF8.GetBytes(originalString);

            var encodedString = Convert.ToBase64String(bytes);

            return encodedString;
        }

        public static string Base64StringDecode(this string encodedString)
        {
            if (string.IsNullOrEmpty(encodedString))
            {
                return null;
            }
            var bytes = Convert.FromBase64String(encodedString);

            var decodedString = Encoding.UTF8.GetString(bytes);

            return decodedString;
        }

        public static string Hash(this string input)
        {
            StringBuilder sb = new StringBuilder();
            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(input));

                foreach (Byte b in result)
                    sb.Append(b.ToString("x2"));
            }

            var resultHash = sb.ToString();

            return resultHash;
        }
    }
}
