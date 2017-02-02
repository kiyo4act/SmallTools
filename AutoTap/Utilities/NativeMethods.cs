using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    class NativeMethods
    {
        [DllImport("KERNEL32.DLL", CharSet = CharSet.Unicode)]
        public static extern uint
            GetPrivateProfileString(string lpAppName,
            string lpKeyName, string lpDefault,
            StringBuilder lpReturnedString, uint nSize,
            string lpFileName);

        [DllImport("KERNEL32.DLL", CharSet = CharSet.Unicode)]
        public static extern uint
            GetPrivateProfileInt(string lpAppName,
            string lpKeyName, int nDefault, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int WritePrivateProfileString(
            string lpApplicationName,
            string lpKeyName,
            string lpstring,
            string lpFileName);
    }
}
