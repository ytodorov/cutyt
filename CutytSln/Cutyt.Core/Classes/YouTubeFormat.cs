using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Classes
{
    public class YouTubeFormat
    {
        public long? Fps { get; set; }

        public string Container { get; set; }

        /// <summary>
        /// 17391138. Bytes.
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// 48.134
        /// </summary>
        public double? Tbr { get; set; }

        /// <summary>
        /// 68.568
        /// </summary>
        public double? Vbr { get; set; }

        /// <summary>
        /// "394"
        /// </summary>
        public string Format_Id { get; set; }

        public long? Width { get; set; }

        /// <summary>
        /// m4a
        /// </summary>
        public string Ext { get; set; }

        public long? Asr { get; set; }

        /// <summary>
        /// 0
        /// </summary>
        public long? Quality { get; set; }

        /// <summary>
        /// "278 - 256x144 (144p)" or "140 - audio only (tiny)"
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// "https://r4---sn-hvcpauxa-nv46.googlevideo.com/videoplayback?expire=1630089966&ei=jt4oYayYDam6x_AP5fy0uAQ&ip=217.79.32.194&id=o-ALMXtkCNAclKnEGlWTl1mkF7NIgY0j7koYWwT57KGRP6&itag=278&aitags=133%2C134%2C135%2C136%2C137%2C160%2C242%2C243%2C244%2C247%2C248%2C278%2C394%2C395%2C396%2C397%2C398%2C399&source=youtube&requiressl=yes&mh=Ae&mm=31%2C29&mn=sn-hvcpauxa-nv46%2Csn-nv47ln7z&ms=au%2Crdu&mv=m&mvi=4&pl=20&initcwndbps=683750&vprv=1&mime=video%2Fwebm&ns=gM6CbbrYuSa07HYS6YZYsTMG&gir=yes&clen=24773651&dur=2890.400&lmt=1629084232666713&mt=1630068054&fvip=4&keepalive=yes&fexp=24001373%2C24007246&c=WEB&txp=5516222&n=MeTdHGWUL8tpv6eM5_Jw&sparams=expire%2Cei%2Cip%2Cid%2Caitags%2Csource%2Crequiressl%2Cvprv%2Cmime%2Cns%2Cgir%2Cclen%2Cdur%2Clmt&sig=AOq0QJ8wRgIhAItpr_ki4AYGOnRsQhyMZ3K840kclHYISBxtBfPw0BKFAiEAgMOJ4rmlvApRdFHcDOphjUTIU6skSQok-tp5VD3tb9s%3D&lsparams=mh%2Cmm%2Cmn%2Cms%2Cmv%2Cmvi%2Cpl%2Cinitcwndbps&lsig=AG3C_xAwRQIhALTINHaKQrB3bQZqmAdYSv08Z_CIv-S3dF6vZOV5sHQLAiBzN5lo8dTvPlx6F4lEniDGzaxTgsa5YoWRM0-eZKvoUg%3D%3D",
        /// </summary>
        public string Url { get; set; }

        public long? Height { get; set; }


        /// <summary>
        /// "vp9"
        /// </summary>
        public string Vcodec { get; set; }

        public string Acodec { get; set; }

        /// <summary>
        ///  "144p"
        /// </summary>
        public string Format_Note { get; set; }

        /// <summary>
        /// This is calculated for UI. Yordan.
        /// </summary>
        public string DownloadSwitchAudioAndVideo { get; set; }

    }
}
