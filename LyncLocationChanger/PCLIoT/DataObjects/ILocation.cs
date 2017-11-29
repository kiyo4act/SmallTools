using PCLIoT.DataObjects;

namespace PCLIoT
{
    public interface ILocation : IUserTable
    {
        double boundaryRadias { get; set; }
        bool inRange { get; set; }
        string LyncID { get; set; }
        string name { get; set; }
        //ILocation parent { get; set; }
        bool received { get; set; }
        double x { get; set; }
        double y { get; set; }
        long BeaconMAC { get; set; }
    }
}