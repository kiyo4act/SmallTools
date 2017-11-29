using Newtonsoft.Json;
using PCLIoT.DataObjects;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PCLIoT
{
    public class WiGigDevice : UserTable, IWiGigDevice
    {
        [JsonProperty(PropertyName = "devicename")]
        public string deviceName { get; set; }
        [JsonProperty(PropertyName = "devicetype")]
        public string deviceType { get; set; }
        [JsonProperty(PropertyName = "primarymacaddress")]
        public string primaryMacAddress { get; set; }
        [JsonProperty(PropertyName = "secondarymacaddress")]
        public string secondaryMacAddress { get; set; }
        int _performanceQuality;
        [JsonProperty(PropertyName = "performancequality")]
        public int performanceQuality
        {
            get { return _performanceQuality; }
            set { SetProperty(ref _performanceQuality, value); }
        }
        
        bool _connect;
        [JsonProperty(PropertyName = "connect")]
        public bool connect {
            get { return _connect; }
            set { SetProperty(ref _connect, value); } }

        static public WiGigDevice create(StationBasicInfoSdk info, int ePerformanceQualitySdk)
        {
            return new WiGigDevice()
            {
                deviceName = System.Text.Encoding.UTF8.GetString(info.deviceName,0, info.ssidLength),
                deviceType = WiGigPortable.DisplayName0((int)info.deviceType),
                primaryMacAddress = BitConverter.ToString(info.primaryMacAddress),
                secondaryMacAddress = BitConverter.ToString(info.secondaryMacAddress),
                performanceQuality = ePerformanceQualitySdk
            };
        }
    }

    //public class WiGigDevices : List<WiGigDevice>
    //{
    //    public WiGigDevices() { }
    //}

    public static class WiGigPortable
    {
        public const int WIGIG_SDK_DEVICE_NAME_VALID_RANGE = 30;
        public const int WIGIG_SDK_MAC_ADDRESS_LEN_IN_BYTES = 6;
        public static string DisplayName(this eDeviceTypeSdk deviceType)
        {
            string[] names = { "Notebook", "Dock", "Invalid", "NOE" };
            return names[(int)deviceType];
        }

        public static string DisplayName0(int id)
        {
            string[] names = { "Notebook", "Dock", "Invalid", "NOE" };
            return names[id];
        }
    }
    //        public delegate void WigGigMACCB( int rowsize);
    [StructLayout(LayoutKind.Sequential)]
    public struct Field_SupportedServicesSdk //True in one of the field indicate on supporting				
    {
        public UInt16 WseHostService;
        public UInt16 WseDeviceService;
        public UInt16 WdeSinkService;
        public UInt16 WdeSourceService;
        public UInt16 P2pIpService;
        public UInt16 reserved;
    };
    public enum eDeviceTypeSdk : byte
    {
        SDK_DEVICE_TYPE_NB,                                         /*!< Notebook / Laptop */
        SDK_DEVICE_TYPE_DOCK,                                       /*!< Docking Station */
        SDK_DEVICE_TYPE_INVALID,
        SDK_DEVICE_TYPE_NUMBER_OF_ELEMENTS = SDK_DEVICE_TYPE_INVALID
    };
    [StructLayout(LayoutKind.Sequential)]
    public struct StationBasicInfoSdk
    {
        /// Station device name as being broadcasts to other stations. 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = WiGigPortable.WIGIG_SDK_DEVICE_NAME_VALID_RANGE)]
        public byte[] deviceName;    // A 0 length information field is used to indicate the wildcard device name.
                                     /// Length in bytes of SSID string
        public byte ssidLength;
        /// Primary MAC address
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = WiGigPortable.WIGIG_SDK_MAC_ADDRESS_LEN_IN_BYTES)]
        public byte[] primaryMacAddress;
        /// Secondary MAC address (Optional)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = WiGigPortable.WIGIG_SDK_MAC_ADDRESS_LEN_IN_BYTES)]
        public byte[] secondaryMacAddress;
        /// Supported services(WSE,WDE,IP...)
        public UInt32 supportedServices;
        /// Device type: (e.g. Notebook)
        public UInt32 deviceType;
    };

    public class TodoItem // for Azure test
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool Complete { get; set; }
    }

}
