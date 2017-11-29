using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesLib
{
    public class NativeMethods
    {
        //接続切断するWin32 API を宣言
        [DllImport("mpr.dll", EntryPoint = "WNetCancelConnection2", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal static extern int WNetCancelConnection2(string lpName, Int32 dwFlags, bool fForce);

        [DllImport("mpr.dll", EntryPoint = "WNetAddConnection2", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal static extern int WNetAddConnection2(ref NETRESOURCE lpNetResource, string lpPassword, string lpUsername, Int32 dwFlags);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        internal static extern bool CloseHandle(IntPtr handle);

        public static WindowsImpersonationContext GetImpersonationContext(string userName, string domainName, string password)
        {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            const int LOGON32_LOGON_NEW_CREDENTIALS = 5;
            //const int LOGON32_LOGON_INTERACTIVE = 2;

            try
            {
                SafeTokenHandle safeTokenHandle;
                var returnValue = LogonUser(userName, domainName, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);

                if (false == returnValue) throw new ArgumentException(string.Format("ログインユーザの偽装に失敗しました。 ErrorCode:{0}", Marshal.GetLastWin32Error()));

                using (safeTokenHandle)
                {
                    var newId = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                    return newId.Impersonate();
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NETRESOURCE
        {
            public int dwScope;
            public int dwType;
            public int dwDisplayType;
            public int dwUsage;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpLocalName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpRemoteName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpComment;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpProvider;
        }

        public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeTokenHandle() : base(true) { }

            [DllImport("kernel32.dll")]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }
    }
}
