using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UtilitiesLib
{
    public class NetworkUtility : IDisposable
    {
        private bool IsConnected { get; set; }
        private string NetworkPath { get; set; }
        private string NetworkUsername { get; set; }
        private string NetworkPassword { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NetworkUtility()
        {
            NetworkPath = string.Empty;
            NetworkUsername = string.Empty;
            NetworkPassword = string.Empty;
            IsConnected = false;
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="networkPath"></param>
        public NetworkUtility(string networkPath) : this()
        {
            NetworkPath = networkPath;
            IsConnected = CheckNetworkConnection(networkPath);
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="networkPath"></param>
        /// <param name="networkUsername"></param>
        public NetworkUtility(string networkPath, string networkUsername) : this(networkPath)
        {
            NetworkUsername = networkUsername;
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="networkPath"></param>
        /// <param name="networkUsername"></param>
        /// <param name="networkPassword"></param>
        public NetworkUtility(string networkPath, string networkUsername, string networkPassword) : this(networkPath, networkUsername)
        {
            NetworkPassword = networkPassword;
        }

        /// <summary>
        /// パスがネットワーク上のものかを調べる
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool CheckNetworkPath()
        {
            bool fResult = false;
            string path = NetworkPath;
            try
            {
                path = Path.GetPathRoot(path);
            }
            catch(Exception)
            {
                return false;    
            }

            
            if (System.Text.RegularExpressions.Regex.IsMatch(path, @"^[A-Z].*", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                // ^[A-Z].* にマッチするパターン
                DriveInfo driveInfo = new DriveInfo(path);
                if (driveInfo.DriveType.Equals(DriveType.Network))
                {
                    // ネットワークドライブだったら
                    fResult = true;
                }
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(path, @"^\\\\.*", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                // ^\\\\.* にマッチするパターン
                fResult = true;
            }

            return fResult;
        }

        public bool CheckNetworkPath(string path)
        {
            NetworkPath = path;
            return CheckNetworkPath();
        }

        /// <summary>
        /// ネットワークパスへアクセス可能か調べる
        /// </summary>
        /// <param name="networkPath"></param>
        /// <returns></returns>
        public bool CheckNetworkConnection(string networkPath)
        {
            bool fResult = false;

            try
            {
                if (Directory.Exists(networkPath))
                {
                    fResult = true;
                    IsConnected = true;
                }
            }
            catch (Exception)
            {
                fResult = false;
                IsConnected = false;
            }

            return fResult;
        }

        /// <summary>
        /// 接続する
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            bool fResult = false;

            NativeMethods.NETRESOURCE netResource = new NativeMethods.NETRESOURCE();
            netResource.dwScope = 0;
            netResource.dwType = 1;
            netResource.dwDisplayType = 0;
            netResource.dwUsage = 0;
            netResource.lpLocalName = "";
            netResource.lpRemoteName = NetworkPath;
            netResource.lpProvider = "";

            try
            {
                fResult = (NativeMethods.WNetCancelConnection2(NetworkPath, 0, true) == 0) ? true : false;
                fResult = (NativeMethods.WNetAddConnection2(ref netResource, NetworkPassword, NetworkUsername, 0) == 0) ? true : false;
            }
            catch (Exception)
            {
                return fResult = false;
            }

            return fResult;
        }

        /// <summary>
        /// 接続する
        /// </summary>
        /// <param name="networkPath"></param>
        /// <returns></returns>
        public bool Connect(string networkPath)
        {
            NetworkPath = networkPath;
            return Connect();
        }

        /// <summary>
        /// 接続する
        /// </summary>
        /// <param name="networkPath"></param>
        /// <param name="networkUsername"></param>
        /// <returns></returns>
        public bool Connect(string networkPath, string networkUsername)
        {
            NetworkPath = networkPath;
            NetworkUsername = networkUsername;
            return Connect();
        }

        /// <summary>
        /// 接続する
        /// </summary>
        /// <param name="networkPath"></param>
        /// <param name="networkUsername"></param>
        /// <param name="networkPassword"></param>
        /// <returns></returns>
        public bool Connect(string networkPath, string networkUsername, string networkPassword)
        {
            NetworkPath = networkPath;
            NetworkUsername = networkUsername;
            NetworkPassword = networkPassword;
            return Connect();
        }

        /// <summary>
        /// 切断する
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            bool fResult = false;

            if(IsConnected)
            {
                try
                {
                    fResult = (NativeMethods.WNetCancelConnection2(NetworkPath, 0, true) == 0) ? true : false;
                }
                catch (Exception)
                {
                    return fResult;
                }
            }
            return fResult;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。
                if (IsConnected)
                {
                    NativeMethods.WNetCancelConnection2(NetworkPath, 0, false);
                    IsConnected = false;
                }

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        ~NetworkUtility() {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(false);
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
