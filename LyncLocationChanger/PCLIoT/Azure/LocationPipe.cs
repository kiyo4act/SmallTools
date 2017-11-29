using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLIoT.Azure
{
    public class LocationPipe : AzureMobileUserPipe<Location>
    {
        public LocationPipe(string username) : base(username)
        {
        }
    }
}
