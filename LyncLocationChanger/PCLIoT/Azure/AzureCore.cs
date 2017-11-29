using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLIoT.Azure
{
    public class AzureCore
    {
        public static MobileServiceClient MobileService =
            new MobileServiceClient(
                    "https://magellanpipe.azure-mobile.net/",
                    "nKakToHNAcqJgRiDEhTajoduSZcPlS61");
    }
}
