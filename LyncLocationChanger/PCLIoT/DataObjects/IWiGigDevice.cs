using PCLIoT.DataObjects;

namespace PCLIoT
{
    public interface IWiGigDevice : IUserTable
    {
        bool connect { get; set; }
        string deviceName { get; set; }
        string deviceType { get; set; }
        int performanceQuality { get; set; }
        string primaryMacAddress { get; set; }
        string secondaryMacAddress { get; set; }
    }
}