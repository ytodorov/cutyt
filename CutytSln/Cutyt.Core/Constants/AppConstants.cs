using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Constants
{
    public static class AppConstants
    {
        public static string YtWorkingDir
        {
            get
            {
                if (Directory.Exists($"E:\\Files"))
                {
                    return "E:\\Files";
                }
                else
                {
                    return "C:\\Files";
                }
            }
        }
    }
}
