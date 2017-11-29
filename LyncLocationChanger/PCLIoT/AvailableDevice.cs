using System;

namespace PCLIoT
{
    public class AvailableDevice
    {
        public byte[] deviceMAC { get; set; }
        public string deviceType { get; set; }
        public string locationTag { get; set; }
        public bool inRange { get; set; } = false;
        //            public double threshold;
        public DateTimeOffset lastAppeared { get; set; }
    }
}
