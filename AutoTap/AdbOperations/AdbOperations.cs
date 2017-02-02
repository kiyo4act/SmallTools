using System;
using System.Diagnostics;
using System.Threading;

namespace AdbOperations
{
    public static class AdbOperation
    {
        public static ProcessStartInfo AdbProcessStartInfo = new ProcessStartInfo
        {
            FileName = "Adb\\adb.exe",
            //WorkingDirectory = @"Adb",
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
        };
        
        public static void Tap(int x, int y)
        {
            Tap(x.ToString(), y.ToString());
        }
        public static void Tap(string x ,string y )
        {
            Debug.WriteLine(string.Format("AdbOperation.Tap({0}, {1})", x, y));

            AdbProcessStartInfo.Arguments = @"shell input tap " + x + " " + y;
            Process process = Process.Start(AdbProcessStartInfo);
            process?.WaitForExit();
        }
        public static void Swipe(int xStart, int yStart, int xEnd, int yEnd, int duration)
        {
            Swipe(xStart.ToString(), yStart.ToString(), xEnd.ToString(), yEnd.ToString(), duration.ToString());
        }
        public static void Swipe(string xStart, string yStart, string xEnd, string yEnd, string duration)
        {
            Debug.WriteLine(string.Format("AdbOperation.Swipe({0}, {1}, {2}, {3}, {4}", xStart, yStart, xEnd, yEnd, duration));

            AdbProcessStartInfo.Arguments = @"shell input swipe " + xStart +" "+ yStart +" "+ xEnd +" "+ yEnd + " " + duration;
            Process process = Process.Start(AdbProcessStartInfo);
            process?.WaitForExit();
        }
        public static void Swipe(int xStart, int yStart, int xEnd, int yEnd)
        {
            Swipe(xStart.ToString(), yStart.ToString(), xEnd.ToString(), yEnd.ToString());
        }
        public static void Swipe(string xStart, string yStart, string xEnd, string yEnd)
        {
            Debug.WriteLine(string.Format("AdbOperation.Swipe({0}, {1}, {2}, {3}", xStart, yStart, xEnd, yEnd));
            AdbProcessStartInfo.Arguments = @"shell input swipe " + xStart + " " + yStart + " " + xEnd + " " + yEnd;
            Process process = Process.Start(AdbProcessStartInfo);
            process?.WaitForExit();
        }
        public static void InputText(string text)
        {
            Debug.WriteLine(string.Format("AdbOperation.InputText({0})", text));
            AdbProcessStartInfo.Arguments = @"shell input text " + text;
            Process process = Process.Start(AdbProcessStartInfo);
            process?.WaitForExit();
        }

        public static void InputKeyEvent(string keyEvent)
        {
            Debug.WriteLine(string.Format("AdbOperation.InputKeyEvent({0})", keyEvent));
            AdbProcessStartInfo.Arguments = @"shell input keyevent " + keyEvent;
            Process process = Process.Start(AdbProcessStartInfo);
            process?.WaitForExit();
        }
        public static void TakeScreenshot()
        {
            TakeScreenshot("/sdcard/screen.png");
        }

        public static void TakeScreenshot(string remotePath)
        {
            Debug.WriteLine(string.Format("AdbOperation.TakeScreenshot({0})", remotePath));
            AdbProcessStartInfo.Arguments = @"shell screencap -p " + remotePath;
            Process process = Process.Start(AdbProcessStartInfo);
            process?.WaitForExit();
        }
        public static void PullFile(string remotePath, string localPath)
        {
            Debug.WriteLine(string.Format("AdbOperation.PullFile({0})", remotePath));
            AdbProcessStartInfo.Arguments = @"pull " + remotePath +" "+ localPath;
            Process process = Process.Start(AdbProcessStartInfo);
            process?.WaitForExit();
            string output = process?.StandardOutput.ReadLine();
            Debug.WriteLine(string.Format("AdbOperation.PullFile: {0}", output));
        }
        public static void RemoveFile(string remotePath)
        {
            Debug.WriteLine(string.Format("AdbOperation.RemoveFile({0})", remotePath));
            AdbProcessStartInfo.Arguments = @"shell rm " + remotePath;
            Process process = Process.Start(AdbProcessStartInfo);
            process?.WaitForExit();
        }

        public static void PushPowerButton()
        {
            Debug.WriteLine("AdbOperation.PushPowerButton()");
            AdbProcessStartInfo.Arguments = @"shell input keyevent KEYCODE_POWER";
            Process process = Process.Start(AdbProcessStartInfo);
            process?.WaitForExit();
        }

        public static int GetBatteryInfo()
        {
            Debug.WriteLine("AdbOperation.GetBatteryInfo()");
            AdbProcessStartInfo.Arguments = @"shell cat /sys/class/power_supply/battery/capacity";
            Process process = Process.Start(AdbProcessStartInfo);
            process?.WaitForExit();
            string output = process?.StandardOutput.ReadLine();
            if (output != null) return int.Parse(output);
            return -1;
        }
    }
}
