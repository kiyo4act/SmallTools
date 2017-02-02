using System.Diagnostics;
using System.Threading;

namespace AdbOperations
{
    public static class OcrOperation
    {
        public static ProcessStartInfo OcrProcessStartInfo = new ProcessStartInfo
        {
            FileName = "Tesseract-OCR\\tesseract.exe",
            //WorkingDirectory = "Tesseract-OCR",
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
        };
        public static void DoOcr(string inputImageFileName, string outputTextFileName)
        {
            Debug.WriteLine("OcrOperation.DoOcr(string, string)");

            OcrProcessStartInfo.Arguments = @inputImageFileName + " " + outputTextFileName;
            Process process = Process.Start(OcrProcessStartInfo);
            process?.WaitForExit();
        }
        public static void DoOcr(string inputImageFileName, string outputTextFileName, string langFileName)
        {
            Debug.WriteLine("OcrOperation.DoOcr(string, string, string)");

            OcrProcessStartInfo.Arguments = @inputImageFileName + " " + outputTextFileName + " -l " + langFileName;
            Process process = Process.Start(OcrProcessStartInfo);
            process?.WaitForExit();
        }      
        public static void DoOcr(string inputImageFileName, string outputTextFileName, string langFileName, string configFileName)
        {
            Debug.WriteLine("OcrOperation.DoOcr(string, string, string, string)");

            OcrProcessStartInfo.Arguments = @inputImageFileName+" "+ outputTextFileName +" -l "+langFileName+" " + configFileName;
            Process process = Process.Start(OcrProcessStartInfo);
            process?.WaitForExit();
        }
    }
}
