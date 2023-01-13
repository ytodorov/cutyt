using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Ads
{
    public class Utils
    {
        public static string GetRandomAdd()
        {
            List<string> list = new List<string>()
            {
                """
    <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal003300250.jpg" alt="pCloud Premium"/></a>
    """,
                """
    <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal004300250.jpg" alt="pCloud Premium"/></a>
    """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal004300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal005300250.jpg" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal005300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal006300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal007300250.jpg" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal008300250.jpg" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal009300250.jpg" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal003300250.jpg" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal004300250.jpg" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal004300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal005300250.jpg" alt="pCloud Premium"/></a><a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal005300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal006300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal007300250.jpg" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal008300250.jpg" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/personal/personal009300250.jpg" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/lifetime/lifetime001300250.jpg" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/lifetime/lifetime002300250.jpg" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/lifetime/lifetime003300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/lifetime/lifetime004300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/lifetime/lifetime005300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/lifetime/lifetime006300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/lifetime/lifetime007300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/lifetime/lifetime008300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/lifetime/lifetime009300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/lifetime/lifetime010300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/lifetime/lifetime011300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/lifetime/lifetime012300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/lifetime/lifetime013300250.png" alt="pCloud Premium"/></a>
                """,
                """
                <a href="https://partner.pcloud.com/r/80424" title="pCloud Premium" target="_blank"><img src="https://partner.pcloud.com/media/banners/lifetime/lifetime014300250.png" alt="pCloud Premium"/></a>
                """
            };

            var randomInt = Random.Shared.Next(0, list.Count);
            var randomString = list[randomInt];
            return randomString;
        }
    }
}
