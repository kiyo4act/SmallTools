using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PCLIoT
{
    public class AvailableBLEBeacon : BindableBase
    {
        ulong _MAC;
        public ulong MAC { get { return _MAC; } set { SetProperty(ref _MAC, value); } }
        byte[] _deviceMAC;
        public byte[] deviceMAC { get { return _deviceMAC; } set { SetProperty(ref _deviceMAC, value); } }
        double _RSSI;
        public double RSSI { get { return _RSSI; } set { SetProperty(ref _RSSI, value); } }
        double _smoothedRSSI;
        public double smoothedRSSI { get { return _smoothedRSSI; } set { SetProperty(ref _smoothedRSSI, value); } }
        bool _inRange = false;
        public bool inRange { get { return _inRange; } set { SetProperty(ref _inRange, value); } }
        bool _received = false;
        public bool received { get { return _received; } set { SetProperty(ref _received, value); } }
        double _threshold = 1;
        public double threshold { get { return _threshold; } set { SetProperty(ref _threshold, value); } }
        DateTimeOffset _lastAppeared;
        public DateTimeOffset lastAppeared { get { return _lastAppeared; } set { SetProperty(ref _lastAppeared, value); } }
        Location _location;
        public Location location { get { return _location; } set { SetProperty(ref _location, value); } }
        byte _DataType;
        public byte DataType { get { return _DataType; } set { SetProperty(ref _DataType, value); } }
        byte _DeviceCategory;
        public byte DeviceCategory { get { return _DeviceCategory; } set { SetProperty(ref _DeviceCategory, value); } }
        byte _Command;
        public byte Command { get { return _Command; } set { SetProperty(ref _Command, value); } }
    }


    public static class BLEBeaconExtension
    {
        public static AvailableBLEBeacon Find(this ObservableCollection<AvailableBLEBeacon> list, ulong MAC)
        {
            var beacons = from beacon in list
                          where beacon.MAC == MAC
                          select beacon;
            if (beacons.Count() > 0)
            {
                return beacons.First();
            }
            else
            {
                return null;
            }
        }
    }

    //public class AvailableBLEBeacons : List<AvailableBLEBeacon>
    //{
    //    public AvailableBLEBeacons() { }
    //}


}
