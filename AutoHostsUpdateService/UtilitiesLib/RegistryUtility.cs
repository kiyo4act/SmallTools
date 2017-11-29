using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesLib
{
    public static class RegistryUtility
    {
        /// <summary>
        /// Create a registry sub key (if it doesn't exist) and write out the registry name/value.
        /// </summary>
        /// <param name="hive"></param>
        /// <param name="strSubKey"></param>
        /// <param name="strName"></param>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static bool FRegistrySetValue(RegistryHive hive, string strSubKey, string strName, string strValue)
        {
            bool fResult = false;

            // strName could be null. If strName is null, the registry name is "(Default)".
            // http://msdn.microsoft.com/en-us/library/2kk9bxk9.aspx
            if ((!String.IsNullOrEmpty(strSubKey)) && (strValue != null))
            {
                try
                {
                    RegistryKey hKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                    RegistryKey hKeySub = hKey.CreateSubKey(strSubKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    if (hKeySub != null)
                    {
                        hKeySub.SetValue(strName, strValue);

                        fResult = true;
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (SecurityException)
                {
                }
                catch (ObjectDisposedException)
                {
                }
                catch (IOException)
                {
                }
            }

            return fResult;
        }

        /// <summary>
        /// Read registry string value.
        /// </summary>
        /// <param name="hive"></param>
        /// <param name="strSubKey"></param>
        /// <param name="strName"></param>
        /// <returns></returns>
        public static string StrRegistryReadValue(RegistryHive hive, string strSubKey, string strName)
        {
            string strValue = String.Empty;

            // strName could be null. If strName is null, the registry name is "(Default)".
            // http://msdn.microsoft.com/en-us/library/fdf576x1.aspx
            if (!String.IsNullOrEmpty(strSubKey))
            {
                try
                {
                    RegistryKey hKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                    RegistryKey hKeySub = hKey.OpenSubKey(strSubKey, RegistryKeyPermissionCheck.ReadSubTree);
                    if (hKeySub != null)
                    {
                        strValue = (string)hKeySub.GetValue(strName);
                        if (strValue == null)
                        {
                            strValue = String.Empty;
                        }
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (SecurityException)
                {
                }
                catch (ObjectDisposedException)
                {
                }
                catch (IOException)
                {
                }
            }

            return strValue;
        }


        /// <summary>
        /// Remove registry value.
        /// </summary>
        /// <param name="hive"></param>
        /// <param name="strSubKey"></param>
        /// <param name="strName"></param>
        /// <returns></returns>
        public static bool FRemoveRegistryValue(RegistryHive hive, string strSubKey, string strName)
        {
            bool fResult = false;

            if (!String.IsNullOrEmpty(strSubKey))
            {
                try
                {
                    RegistryKey hKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                    RegistryKey hKeySub = hKey.OpenSubKey(strSubKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    if (hKeySub != null)
                    {
                        hKeySub.DeleteValue(strName);
                        fResult = true;
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (SecurityException)
                {
                }
                catch (ObjectDisposedException)
                {
                }
            }

            return fResult;
        }


        /// <summary>
        /// Read registry DWORD value.
        /// </summary>
        /// <param name="hive"></param>
        /// <param name="strSubKey"></param>
        /// <param name="strName"></param>
        /// <param name="dwValue"></param>
        /// <returns></returns>
        public static bool FRegistryReadDwordValue(RegistryHive hive, string strSubKey, string strName, out int dwValue)
        {
            bool fResult = false;
            dwValue = 0;

            if (!String.IsNullOrEmpty(strSubKey))
            {
                try
                {
                    RegistryKey hKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                    RegistryKey hKeySub = hKey.OpenSubKey(strSubKey, RegistryKeyPermissionCheck.ReadSubTree);
                    if (hKeySub != null)
                    {
                        var value = hKeySub.GetValue(strName);
                        if (value != null)
                        {
                            fResult = Int32.TryParse(value.ToString(), out dwValue);
                        }
                    }
                }
                catch (ArgumentNullException)
                {
                }
                catch (ArgumentException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (SecurityException)
                {
                }
                catch (ObjectDisposedException)
                {
                }
                catch (IOException)
                {
                }
            }

            return fResult;
        }

        /// <summary>
        /// Write registry DWORD value.
        /// </summary>
        /// <param name="hive"></param>
        /// <param name="strSubKey"></param>
        /// <param name="strName"></param>
        /// <param name="dwValue"></param>
        /// <returns></returns>
        public static bool FRegistrySetDwordValue(RegistryHive hive, string strSubKey, string strName, uint dwValue)
        {
            bool fResult = false;

            if (!String.IsNullOrEmpty(strSubKey))
            {
                try
                {
                    RegistryKey hKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                    RegistryKey hKeySub = hKey.CreateSubKey(strSubKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    if (hKeySub != null)
                    {
                        hKeySub.SetValue(strName, dwValue, RegistryValueKind.DWord);

                        fResult = true;
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (SecurityException)
                {
                }
                catch (ObjectDisposedException)
                {
                }
                catch (IOException)
                {
                }
            }

            return fResult;
        }

        /// <summary>
        /// Read registry Binary value.
        /// </summary>
        /// <param name="hive"></param>
        /// <param name="strSubKey"></param>
        /// <param name="strName"></param>
        /// <param name="dwValue"></param>
        /// <returns></returns>
        public static byte[] RGBRegistryReadBinaryValue(RegistryHive hive, string strSubKey, string strName)
        {
            if (!String.IsNullOrEmpty(strSubKey))
            {
                try
                {
                    RegistryKey hKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                    RegistryKey hKeySub = hKey.OpenSubKey(strSubKey, RegistryKeyPermissionCheck.ReadSubTree);
                    if (hKeySub != null)
                    {
                        byte[] value = (byte[]) hKeySub.GetValue(strName);
                        if (value != null)
                        {
                            
                            return value;
                        }
                    }
                }
                catch (ArgumentNullException)
                {
                }
                catch (ArgumentException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (SecurityException)
                {
                }
                catch (ObjectDisposedException)
                {
                }
                catch (IOException)
                {
                }
            }
            return null;
        }

        /// <summary>
        /// Write registry Binary value.
        /// </summary>
        /// <param name="hive"></param>
        /// <param name="strSubKey"></param>
        /// <param name="strName"></param>
        /// <param name="dwValue"></param>
        /// <returns></returns>
        public static bool FRegistrySetBinaryValue(RegistryHive hive, string strSubKey, string strName, byte[] dwValue)
        {
            bool fResult = false;

            if (dwValue != null)
            {
                try
                {
                    RegistryKey hKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                    RegistryKey hKeySub = hKey.CreateSubKey(strSubKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    if (hKeySub != null)
                    {
                        hKeySub.SetValue(strName, dwValue, RegistryValueKind.Binary);

                        fResult = true;
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (SecurityException)
                {
                }
                catch (ObjectDisposedException)
                {
                }
                catch (IOException)
                {
                }
            }

            return fResult;
        }



        /// <summary>
        /// Remove registry key tree.
        /// </summary>
        /// <param name="hive"></param>
        /// <param name="strSubKey"></param>
        /// <returns></returns>
        public static bool FRemoveRegistryKeyTree(RegistryHive hive, string strSubKey)
        {
            bool fResult = false;

            if (!String.IsNullOrEmpty(strSubKey))
            {
                try
                {
                    RegistryKey hKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                    hKey.DeleteSubKeyTree(strSubKey);
                    fResult = true;
                }
                catch (ArgumentException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (SecurityException)
                {
                }
                catch (ObjectDisposedException)
                {
                }
            }

            return fResult;
        }
    }
}
