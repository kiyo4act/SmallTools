using AdbOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace AutoTapGUI
{
    class AndroidDevice
    {
        public enum UnlockTypeEnum
        {
            None,
            Pin,
            Passowrd,
            PinWithoutEnter
        }
        // Area
        public int CheckInAreaXStart { get; set; }
        public int CheckInAreaYStart { get; set; }
        public int CheckInAreaXEnd { get; set; }
        public int CheckInAreaYEnd { get; set; }
        public int LinkAreaXStart { get; set; }
        public int LinkAreaYStart { get; set; }
        public int LinkAreaXEnd { get; set; }
        public int LinkAreaYEnd { get; set; }
        public int WindowCloseAreaXStart { get; set; }
        public int WindowCloseAreaYStart { get; set; }
        public int WindowCloseAreaXEnd { get; set; }
        public int WindowCloseAreaYEnd { get; set; }
        public int BagButtonAreaXStart { get; set; }
        public int BagButtonAreaYStart { get; set; }
        public int BagButtonAreaXEnd { get; set; }
        public int BagButtonAreaYEnd { get; set; }
        public int BatteryButtonAreaXStart { get; set; }
        public int BatteryButtonAreaYStart { get; set; }
        public int BatteryButtonAreaXEnd { get; set; }
        public int BatteryButtonAreaYEnd { get; set; }
        public int Charge50ButtonAreaXStart { get; set; }
        public int Charge50ButtonAreaYStart { get; set; }
        public int Charge50ButtonAreaXEnd { get; set; }
        public int Charge50ButtonAreaYEnd { get; set; }
        public int Charge150ButtonAreaXStart { get; set; }
        public int Charge150ButtonAreaYStart { get; set; }
        public int Charge150ButtonAreaXEnd { get; set; }
        public int Charge150ButtonAreaYEnd { get; set; }
        public int Charge300ButtonAreaXStart { get; set; }
        public int Charge300ButtonAreaYStart { get; set; }
        public int Charge300ButtonAreaXEnd { get; set; }
        public int Charge300ButtonAreaYEnd { get; set; }
        public int CloseButtonAreaXStart { get; set; }
        public int CloseButtonAreaYStart { get; set; }
        public int CloseButtonAreaXEnd { get; set; }
        public int CloseButtonAreaYEnd { get; set; }
        public int ErrorButtonAreaXStart { get; set; }
        public int ErrorButtonAreaYStart { get; set; }
        public int ErrorButtonAreaXEnd { get; set; }
        public int ErrorButtonAreaYEnd { get; set; }
        public int RadarButtonAreaXStart { get; set; }
        public int RadarButtonAreaYStart { get; set; }
        public int RadarButtonAreaXEnd { get; set; }
        public int RadarButtonAreaYEnd { get; set; }
        public int Radar1ButtonAreaXStart { get; set; }
        public int Radar1ButtonAreaYStart { get; set; }
        public int Radar1ButtonAreaXEnd { get; set; }
        public int Radar1ButtonAreaYEnd { get; set; }
        public int RadarButtonsOffsetY { get; set; }
        public int UseRadarButtonAreaXStart { get; set; }
        public int UseRadarButtonAreaYStart { get; set; }
        public int UseRadarButtonAreaXEnd { get; set; }
        public int UseRadarButtonAreaYEnd { get; set; }
        public int RadarCloseButtonAreaXStart { get; set; }
        public int RadarCloseButtonAreaYStart { get; set; }
        public int RadarCloseButtonAreaXEnd { get; set; }
        public int RadarCloseButtonAreaYEnd { get; set; }
        public int ReturnMaintenanceButtonAreaXStart { get; set; }
        public int ReturnMaintenanceButtonAreaYStart { get; set; }
        public int ReturnMaintenanceButtonAreaXEnd { get; set; }
        public int ReturnMaintenanceButtonAreaYEnd { get; set; }

        // Interval
        public int WindowOpenInterval { get; set; }
        public int WindowCloseInterval { get; set; }
        public int WindowCloseAnotherInterval { get; set; }
        public int CheckInIntervalMin { get; set; }
        public int CheckInIntervalMax { get; set; }
        public int MaintainLinkIntervalMin { get; set; }
        public int MaintainLinkIntervalMax { get; set; }
        public int ChargeBattelyInterval { get; set; }
        public int UnlockPowerInterval { get; set; }
        public int UnlockSwipeInterval { get; set; }
        public int UnlockInputInterval { get; set; }
        public int WakeupInterval { get; set; }
        public int CloseButtonInterval { get; set; }
        public int ErrorButtonInterval { get; set; }
        public int ChargeButtonInterval { get; set; }
        public int BattryButtonInterval { get; set; }
        public int BagButtonInterval { get; set; }
        public int DoCheckInInterval { get; set; }
        public int CloseCheckInInterval { get; set; }
        public int TakeScreenShotInterval { get; set; }
        public int PullFileInterval { get; set; }
        public int RemoveFileInterval { get; set; }
        public int RadarButtonInterval { get; set; }
        public int RadarCheckInInterval { get; set; }
        public int RadarCloseButtonInterval { get; set; }
        public int RadarCheckInTryNum { get; set; }
        public int RadarCheckInTryTime { get; set; }
        public int RadarCheckInTryIntervalMin { get; set; }
        public int RadarCheckInTryIntervalMax { get; set; }
        public int RadarUseableNum { get; set; }
        public int ToughRecoveryTimeMin { get; set; }
        public int ToughRecoveryTimeMax { get; set; }
        public int ToughCheckAliveIntervalMin { get; set; }
        public int ToughCheckAliveIntervalMax { get; set; }

        // Image
        public int ScreenShotTrimX { get; set; }
        public int ScreenShotTrimY { get; set; }
        public int ScreenShotTrimWidth { get; set; }
        public int ScreenShotTrimHeight { get; set; }
        public string ComparedLinkImagePath { get; set; }
        public string ComparedErrorImagePath { get; set; }
        public string ComparedGreetingImagePath { get; set; }
        public string ComparedLoadingImagePathA { get; set; }
        public string ComparedLoadingImagePathB { get; set; }
        public string ComparedMaintenanceImagePath { get; set; }
        public string ComparedOpeningImagePath { get; set; }
        public string ComparedTitleImagePath { get; set; }
        public string ComparedTimelineImagePath { get; set; }
        public string ScreenShotRemotePath { get; set; }
        public string ScreenShotLocalPath { get; set; }
        public string TrimImageLocalPath { get; set; }
        public string OcrTextLocalPath { get; set; }

        // Threshold
        public double PatternMatchingThreshold { get; set; }
        public int BatteryEnoughThreshold { get; set; }
        public int BatteryMaxThreshold { get; set; }
        public int NoRecoverThreshold { get; set; }

        // Unlock
        public UnlockTypeEnum UnlockType { get; set; }
        public string UnlockPassword { get; set; }
        public int UnlockXStart { get; set; }
        public int UnlockYStart { get; set; }
        public int UnlockXEnd { get; set; }
        public int UnlockYEnd { get; set; }
        public int UnlockDuration { get; set; }

        public AndroidDevice(string filePath)
        {
            InifileUtils ini = new InifileUtils(filePath);
            
            // Area
            // Check In button area
            CheckInAreaXStart = ini.getValueInt("Area", "CheckInAreaXStart");
            CheckInAreaYStart = ini.getValueInt("Area", "CheckInAreaYStart");
            CheckInAreaXEnd = ini.getValueInt("Area", "CheckInAreaXEnd");
            CheckInAreaYEnd = ini.getValueInt("Area", "CheckInAreaYEnd");

            // Link button area
            LinkAreaXStart = ini.getValueInt("Area", "LinkAreaXStart");
            LinkAreaYStart = ini.getValueInt("Area", "LinkAreaYStart");
            LinkAreaXEnd = ini.getValueInt("Area", "LinkAreaXEnd");
            LinkAreaYEnd = ini.getValueInt("Area", "LinkAreaYEnd");

            // Window Close area
            WindowCloseAreaXStart = ini.getValueInt("Area", "WindowCloseAreaXStart");
            WindowCloseAreaYStart = ini.getValueInt("Area", "WindowCloseAreaYStart");
            WindowCloseAreaXEnd = ini.getValueInt("Area", "WindowCloseAreaXEnd");
            WindowCloseAreaYEnd = ini.getValueInt("Area", "WindowCloseAreaYEnd");

            // Bag Button area
            BagButtonAreaXStart = ini.getValueInt("Area", "BagButtonAreaXStart");
            BagButtonAreaYStart = ini.getValueInt("Area", "BagButtonAreaYStart");
            BagButtonAreaXEnd = ini.getValueInt("Area", "BagButtonAreaXEnd");
            BagButtonAreaYEnd = ini.getValueInt("Area", "BagButtonAreaYEnd");

            // Battery Button area
            BatteryButtonAreaXStart = ini.getValueInt("Area", "BatteryButtonAreaXStart");
            BatteryButtonAreaYStart = ini.getValueInt("Area", "BatteryButtonAreaYStart");
            BatteryButtonAreaXEnd = ini.getValueInt("Area", "BatteryButtonAreaXEnd");
            BatteryButtonAreaYEnd = ini.getValueInt("Area", "BatteryButtonAreaYEnd");

            // Charge 50 Button area
            Charge50ButtonAreaXStart = ini.getValueInt("Area", "Charge50ButtonAreaXStart");
            Charge50ButtonAreaYStart = ini.getValueInt("Area", "Charge50ButtonAreaYStart");
            Charge50ButtonAreaXEnd = ini.getValueInt("Area", "Charge50ButtonAreaXEnd");
            Charge50ButtonAreaYEnd = ini.getValueInt("Area", "Charge50ButtonAreaYEnd");

            // Charge 150 Button area
            Charge150ButtonAreaXStart = ini.getValueInt("Area", "Charge150ButtonAreaXStart");
            Charge150ButtonAreaYStart = ini.getValueInt("Area", "Charge150ButtonAreaYStart");
            Charge150ButtonAreaXEnd = ini.getValueInt("Area", "Charge150ButtonAreaXEnd");
            Charge150ButtonAreaYEnd = ini.getValueInt("Area", "Charge150ButtonAreaYEnd");

            // Charge 300 Button area
            Charge300ButtonAreaXStart = ini.getValueInt("Area", "Charge300ButtonAreaXStart");
            Charge300ButtonAreaYStart = ini.getValueInt("Area", "Charge300ButtonAreaYStart");
            Charge300ButtonAreaXEnd = ini.getValueInt("Area", "Charge300ButtonAreaXEnd");
            Charge300ButtonAreaYEnd = ini.getValueInt("Area", "Charge300ButtonAreaYEnd");

            // Close Button area
            CloseButtonAreaXStart = ini.getValueInt("Area", "CloseButtonAreaXStart");
            CloseButtonAreaYStart = ini.getValueInt("Area", "CloseButtonAreaYStart");
            CloseButtonAreaXEnd = ini.getValueInt("Area", "CloseButtonAreaXEnd");
            CloseButtonAreaYEnd = ini.getValueInt("Area", "CloseButtonAreaYEnd");

            // Error Button area
            ErrorButtonAreaXStart = ini.getValueInt("Area", "ErrorButtonAreaXStart");
            ErrorButtonAreaYStart = ini.getValueInt("Area", "ErrorButtonAreaYStart");
            ErrorButtonAreaXEnd = ini.getValueInt("Area", "ErrorButtonAreaXEnd");
            ErrorButtonAreaYEnd = ini.getValueInt("Area", "ErrorButtonAreaYEnd");

            // Radar Button area
            RadarButtonAreaXStart = ini.getValueInt("Area", "RadarButtonAreaXStart");
            RadarButtonAreaYStart = ini.getValueInt("Area", "RadarButtonAreaYStart");
            RadarButtonAreaXEnd = ini.getValueInt("Area", "RadarButtonAreaXEnd");
            RadarButtonAreaYEnd = ini.getValueInt("Area", "RadarButtonAreaYEnd");
            
            // Radar 1 Button area
            Radar1ButtonAreaXStart = ini.getValueInt("Area", "Radar1ButtonAreaXStart");
            Radar1ButtonAreaYStart = ini.getValueInt("Area", "Radar1ButtonAreaYStart");
            Radar1ButtonAreaXEnd = ini.getValueInt("Area", "Radar1ButtonAreaXEnd");
            Radar1ButtonAreaYEnd = ini.getValueInt("Area", "Radar1ButtonAreaYEnd");
            RadarButtonsOffsetY = ini.getValueInt("Area", "RadarButtonsOffsetY");

            // Radar Close Button area
            RadarCloseButtonAreaXStart = ini.getValueInt("Area", "RadarCloseButtonAreaXStart");
            RadarCloseButtonAreaYStart = ini.getValueInt("Area", "RadarCloseButtonAreaYStart");
            RadarCloseButtonAreaXEnd = ini.getValueInt("Area", "RadarCloseButtonAreaXEnd");
            RadarCloseButtonAreaYEnd = ini.getValueInt("Area", "RadarCloseButtonAreaYEnd");

            // Radar Close Button area
            ReturnMaintenanceButtonAreaXStart = ini.getValueInt("Area", "ReturnMaintenanceButtonAreaXStart");
            ReturnMaintenanceButtonAreaYStart = ini.getValueInt("Area", "ReturnMaintenanceButtonAreaYStart");
            ReturnMaintenanceButtonAreaXEnd = ini.getValueInt("Area", "ReturnMaintenanceButtonAreaXEnd");
            ReturnMaintenanceButtonAreaYEnd = ini.getValueInt("Area", "ReturnMaintenanceButtonAreaYEnd");

            // Interval
            // Window Close interval
            WindowOpenInterval = ini.getValueInt("Interval", "WindowOpenInterval");
            WindowCloseInterval = ini.getValueInt("Interval", "WindowCloseInterval");
            WindowCloseAnotherInterval = ini.getValueInt("Interval", "WindowCloseAnotherInterval");

            // Next Check In interval
            CheckInIntervalMin = ini.getValueInt("Interval", "CheckInIntervalMin");
            CheckInIntervalMax = ini.getValueInt("Interval", "CheckInIntervalMax");

            // Maintain Link interval
            MaintainLinkIntervalMin = ini.getValueInt("Interval", "MaintainLinkIntervalMin");
            MaintainLinkIntervalMax = ini.getValueInt("Interval", "MaintainLinkIntervalMax");

            // Charge Battely interval
            ChargeBattelyInterval = ini.getValueInt("Interval", "ChargeBattelyInterval");

            // Unlock Interval
            UnlockPowerInterval = ini.getValueInt("Interval", "UnlockPowerInterval");
            UnlockSwipeInterval = ini.getValueInt("Interval", "UnlockSwipeInterval");
            UnlockInputInterval = ini.getValueInt("Interval", "UnlockInputInterval");

            // Wakeup Interval
            WakeupInterval = ini.getValueInt("Interval", "WakeupInterval");

            // Close Button Interval
            CloseButtonInterval = ini.getValueInt("Interval", "CloseButtonInterval");

            // Error Button Interval
            ErrorButtonInterval = ini.getValueInt("Interval", "ErrorButtonInterval");

            // Charge Button Interval
            ChargeButtonInterval = ini.getValueInt("Interval", "ChargeButtonInterval");

            // Battry Button Interval
            BattryButtonInterval = ini.getValueInt("Interval", "BattryButtonInterval");

            // Bag Button Interval
            BagButtonInterval = ini.getValueInt("Interval", "BagButtonInterval");

            // CheckIn flow Interval
            DoCheckInInterval = ini.getValueInt("Interval", "DoCheckInInterval");
            CloseCheckInInterval = ini.getValueInt("Interval", "CloseCheckInInterval");

            // ScreenShot Interval
            TakeScreenShotInterval = ini.getValueInt("Interval", "TakeScreenShotInterval");
            PullFileInterval = ini.getValueInt("Interval", "PullFileInterval");
            RemoveFileInterval = ini.getValueInt("Interval", "RemoveFileInterval");

            // Radar Button Interval
            RadarButtonInterval = ini.getValueInt("Interval", "RadarButtonInterval");

            // Radar Check in Interval
            RadarCheckInInterval = ini.getValueInt("Interval", "RadarCheckInInterval");
            
            // Radar Close Button Interval
            RadarCloseButtonInterval = ini.getValueInt("Interval", "RadarCloseButtonInterval");

            // Radar Check in try
            RadarCheckInTryNum = ini.getValueInt("Interval", "RadarCheckInTryNum");
            RadarCheckInTryTime = ini.getValueInt("Interval", "RadarCheckInTryTime");
            RadarCheckInTryIntervalMin = ini.getValueInt("Interval", "RadarCheckInTryIntervalMin");
            RadarCheckInTryIntervalMax = ini.getValueInt("Interval", "RadarCheckInTryIntervalMax");

            // Radar Useable num
            RadarUseableNum = ini.getValueInt("Interval", "RadarUseableNum");

            // Tough Recovery Time
            ToughRecoveryTimeMin = ini.getValueInt("Interval", "ToughRecoveryTimeMin");
            ToughRecoveryTimeMax = ini.getValueInt("Interval", "ToughRecoveryTimeMax");

            // Smart Check Alive Interval
            ToughCheckAliveIntervalMin = ini.getValueInt("Interval", "ToughCheckAliveIntervalMin");
            ToughCheckAliveIntervalMax = ini.getValueInt("Interval", "ToughCheckAliveIntervalMax");

            // Image
            // ScreenShot Info
            ScreenShotTrimX = ini.getValueInt("Image", "ScreenShotTrimX");
            ScreenShotTrimY = ini.getValueInt("Image", "ScreenShotTrimY");
            ScreenShotTrimWidth = ini.getValueInt("Image", "ScreenShotTrimWidth");
            ScreenShotTrimHeight = ini.getValueInt("Image", "ScreenShotTrimHeight");

            // ScreenShot Path
            ScreenShotRemotePath = ini.getValueString("Image", "ScreenShotRemotePath");
            ScreenShotLocalPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\LWR\AutoTap\" + ini.getValueString("Image", "ScreenShotLocalPath");

            // OCR Info
            TrimImageLocalPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\LWR\AutoTap\" + ini.getValueString("Image", "TrimImageLocalPath");
            OcrTextLocalPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\LWR\AutoTap\" + ini.getValueString("Image", "OcrTextLocalPath");

            // Compared Image Path
            ComparedLinkImagePath = ini.getValueString("Image", "ComparedLinkImagePath");
            ComparedErrorImagePath = ini.getValueString("Image", "ComparedErrorImagePath");
            ComparedGreetingImagePath = ini.getValueString("Image", "ComparedGreetingImagePath");
            ComparedLoadingImagePathA = ini.getValueString("Image", "ComparedLoadingImagePathA");
            ComparedLoadingImagePathB = ini.getValueString("Image", "ComparedLoadingImagePathB");
            ComparedMaintenanceImagePath = ini.getValueString("Image", "ComparedMaintenanceImagePath");
            ComparedOpeningImagePath = ini.getValueString("Image", "ComparedOpeningImagePath");
            ComparedTitleImagePath = ini.getValueString("Image", "ComparedTitleImagePath");
            ComparedTimelineImagePath = ini.getValueString("Image", "ComparedTimelineImagePath");

            // Threshold
            // Pattern matching threshold
            PatternMatchingThreshold = double.Parse(ini.getValueString("Threshold", "PatternMatchingThreshold"));

            // Battely threshold
            BatteryEnoughThreshold = ini.getValueInt("Threshold", "BatteryEnoughThreshold");
            BatteryMaxThreshold = ini.getValueInt("Threshold", "BatteryMaxThreshold");

            // No Recover threshold
            NoRecoverThreshold = ini.getValueInt("Threshold", "NoRecoverThreshold");

            // Unlock
            // Unlock
            switch (ini.getValueString("Unlock", "UnlockType").ToUpper())
            {
                case "PIN":
                    UnlockType = UnlockTypeEnum.Pin;
                    break;
                case "PASSWORD":
                    UnlockType = UnlockTypeEnum.Passowrd;
                    break;
                case "NONE":
                    UnlockType = UnlockTypeEnum.None;
                    break;
                case "PINWITHOUTENTER":
                    UnlockType = UnlockTypeEnum.PinWithoutEnter;
                    break;
                default:
                    UnlockType = UnlockTypeEnum.None;
                    break;
            }
            
            UnlockPassword = ini.getValueString("Unlock", "UnlockPassword");
            UnlockXStart = ini.getValueInt("Unlock", "UnlockXStart");
            UnlockYStart = ini.getValueInt("Unlock", "UnlockYStart");
            UnlockXEnd = ini.getValueInt("Unlock", "UnlockXEnd");
            UnlockYEnd = ini.getValueInt("Unlock", "UnlockYEnd");
            UnlockDuration = ini.getValueInt("Unlock", "UnlockDuration");
        }
    }
}
