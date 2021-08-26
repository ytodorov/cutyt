using Cutyt.Core.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Rebus.Replies
{
    public class DownloadedFilesReply
    {
        public List<DownloadFileViewModel> Files { get; set; } = new List<DownloadFileViewModel>();
    }
}
