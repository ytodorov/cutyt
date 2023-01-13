using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Rebus.Jobs
{
    public class DownloadLinkRequestViewModel
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

        public string AudioFormat { get; set; }

        public string Title { get; set; }

        public string SignalrId { get; set; }
    }
}
