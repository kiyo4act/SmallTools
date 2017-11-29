using PCLIoT.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLIoT
{
    public class Location : UserTable, ILocation
    {
        string _name;
        public string name { get { return _name; } set { SetProperty(ref _name, value); } }
        bool _received = false;
        public bool received { get { return _received; } set { SetProperty(ref _received, value); } }
        bool _inRange = false;
        public bool inRange { get { return _inRange; } set { SetProperty(ref _inRange, value); } }
        double _x;
        public double x { get { return _x; } set { SetProperty(ref _x, value); } }
        double _y;
        public double y { get { return _y; } set { SetProperty(ref _y, value); } }
        string _LyncID;
        public string LyncID { get { return _LyncID; } set { SetProperty(ref _LyncID, value); } }
        double _boundaryRadias;
        public double boundaryRadias { get { return _boundaryRadias; } set { SetProperty(ref _boundaryRadias, value); } }
        //ILocation _parent;
        //public ILocation parent { get { return _parent; } set { SetProperty(ref _parent, value); } }
        long _BeaconMAC;
        public long BeaconMAC { get { return _BeaconMAC; } set { SetProperty(ref _BeaconMAC, value); } }
        //AvailableBLEBeacon _beacon;
        //public AvailableBLEBeacon beacon { get { return _beacon; } set { SetProperty(ref _beacon, value); } }
    }
    //public class Locations : List<Location>
    //{
    //    public Locations() { }
    //}
}

