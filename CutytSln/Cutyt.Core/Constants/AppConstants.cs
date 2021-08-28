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
            // TO DO. use Z:
            get
            {
                if (Directory.Exists($"E:\\Files"))
                {
                    return $"E:\\Files"; // Cloud - Azure Files
                }
                else
                {
                    return $"C:\\Files"; // Cloud -  Azure Files
                }
            }
        }

        public static string ServiceBusConnectionString
        {
            // TO DO. use Z:
            get
            {
                if (Environment.MachineName.Equals("YTODOROV-NB", StringComparison.InvariantCultureIgnoreCase))
                {
                    return $"Endpoint=sb://cutytdev.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Ful/1J95IApsHZk5v20D05fudoJay8XBOZb5P4FDaPw="; // Cloud - Azure Files
                }
                else
                {
                    return $"Endpoint=sb://cutytprod.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=KbksWoFC5fMnLeuc6lB3BGRHLp6XipmCFwfa7ST8CDc="; // Cloud -  Azure Files
                }
            }
        }
    }
}
