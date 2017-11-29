using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLIoT
{
    public class DeviceBinding
    {
        public ulong BLEMAC { get; set; }
        public string DeviceMAC { get; set; }
        public DateTimeOffset boundDatetime { get; set; }
        public bool autoConnect { get; set; }
    }
}
