using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ServiceProcess;
using UtilitiesLib;
using System.Timers;
using System.Windows.Threading;

namespace AutoHostsUpdateGUI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {

        private string InternalHostsSaveTarget { get; set; } // 内部Hostsファイルパス
        private string ExternalHostsSaveTarget { get; set; } // 外部Hostsファイルパス

        private bool IsApplyInExternalHosts { get; set; }   // 外部HostsファイルにIP情報を適用するか
        private bool IsUpdateInternalHosts { get; set; }    // ローカルのHostsファイルを更新するか
        private bool IsNetworkAuthentication { get; set; }  // ネットワーク認証をするか

        private string NetworkAuthenticationUsername { get; set; } // ネットワーク認証のユーザー名
        private string NetworkAuthenticationPassword { get; set; } // ネットワーク認証のパスワード

        private bool IsEnoughRequiredInternalHosts { get; set; }
        private bool IsEnoughRequiredExternalHosts { get; set; }

        private ServiceController serviceController = null;
        private DispatcherTimer timer = null;

        public MainWindow()
        {
            InitializeComponent();
            
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        private void MainWindow_Loaded( object sender, RoutedEventArgs e)
        {
            LoadRegistry();

            applyButton.IsEnabled = false;

            IsEnoughRequiredInternalHosts = true;
            IsEnoughRequiredExternalHosts = true;

            timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(DispatcherTimer_Tick);
            timer.Start();
        }

        /// <summary>
        /// レジストリ情報を読み込む
        /// </summary>
        private void LoadRegistry()
        {
            InternalHostsSaveTarget = RegistryUtility.StrRegistryReadValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrInternalHostsTargetRegistryName);
            ExternalHostsSaveTarget = RegistryUtility.StrRegistryReadValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrExternalHostsTargetRegistryName);

            NetworkAuthenticationUsername = RegistryUtility.StrRegistryReadValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrNetworkAuthenticationUsernameName);

            int value = 0;
            if (RegistryUtility.FRegistryReadDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrIsApplyInExternalHostsRegistryName, out value) == true)
            {
                IsApplyInExternalHosts = (value == 0) ? false : true;
            }

            if (RegistryUtility.FRegistryReadDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrIsUpdateInternalHostsRegistryName, out value) == true)
            {
                IsUpdateInternalHosts = (value == 0) ? false : true;
            }

            if (RegistryUtility.FRegistryReadDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrIsNetworkAuthenticationName, out value) == true)
            {
                IsNetworkAuthentication = (value == 0) ? false : true;
            }

            internalHostsPathTextBox.Text = InternalHostsSaveTarget;
            externalHostsPathTextBox.Text = ExternalHostsSaveTarget;
            networkAuthenticationUsernameTextBox.Text = NetworkAuthenticationUsername;
            networkAuthenticationPasswordPasswordBox.Password = CryptUtility.DecryptString(RegistryUtility.RGBRegistryReadBinaryValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrNetworkAuthenticationPasswordName));
            internalHostsPathCheckBox.IsChecked = IsUpdateInternalHosts;
            externalHostsPathCheckBox.IsChecked = IsApplyInExternalHosts;
            networkAuthenticationCheckBox.IsChecked = IsNetworkAuthentication;
        }
        /// <summary>
        /// 現在の設定をレジストリに保存する
        /// </summary>
        private void SaveRegistry()
        {
            // ExternalHostsTarget
            RegistryUtility.FRegistrySetValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrExternalHostsTargetRegistryName, ExternalHostsSaveTarget);
            // InternalHostsTarget
            RegistryUtility.FRegistrySetValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrInternalHostsTargetRegistryName, InternalHostsSaveTarget);
            // ApplyInExternalHosts (Enabled)
            
            RegistryUtility.FRegistrySetDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrIsApplyInExternalHostsRegistryName, (IsApplyInExternalHosts) ? (uint)1 : (uint)0);
            // UpdateInternalHosts (Enabled)
            RegistryUtility.FRegistrySetDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrIsUpdateInternalHostsRegistryName, (IsUpdateInternalHosts) ? (uint)1 : (uint)0);
            // NetworkAuthentication (Disabled)
            RegistryUtility.FRegistrySetDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrIsNetworkAuthenticationName, (IsNetworkAuthentication) ? (uint)1 : (uint)0);
            // NetworkAuthenticationUsername (Empty)
            RegistryUtility.FRegistrySetValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrNetworkAuthenticationUsernameName, NetworkAuthenticationUsername);
            // NetworkAuthenticationPassword (Empty)
            RegistryUtility.FRegistrySetBinaryValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrNetworkAuthenticationPasswordName, CryptUtility.EncryptString(NetworkAuthenticationPassword));
        }

        private XmlResources LoadRegistryToXmlResources()
        {
            string internalHostsSaveTarget = RegistryUtility.StrRegistryReadValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrInternalHostsTargetRegistryName);
            string externalHostsSaveTarget = RegistryUtility.StrRegistryReadValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrExternalHostsTargetRegistryName);
            string networkAuthenticationUsername = RegistryUtility.StrRegistryReadValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrNetworkAuthenticationUsernameName);
            byte[] networkAuthenticationPassword = RegistryUtility.RGBRegistryReadBinaryValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrNetworkAuthenticationPasswordName);
            int isUpdateInternalHosts = 0;
            int isApplyInExternalHosts = 0;
            int isNetworkAuthentication = 0;

            RegistryUtility.FRegistryReadDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrIsUpdateInternalHostsRegistryName, out isUpdateInternalHosts);
            RegistryUtility.FRegistryReadDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrIsApplyInExternalHostsRegistryName, out isApplyInExternalHosts);
            RegistryUtility.FRegistryReadDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrIsNetworkAuthenticationName, out isNetworkAuthentication);

            XmlResources xmlResources = new XmlResources(internalHostsSaveTarget, externalHostsSaveTarget, isUpdateInternalHosts, isApplyInExternalHosts, isNetworkAuthentication, networkAuthenticationUsername, networkAuthenticationPassword);
            return xmlResources;
        }

        private void SaveRegistryFromXmlResources(XmlResources xmlResources)
        {
            // ExternalHostsTarget
            RegistryUtility.FRegistrySetValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrExternalHostsTargetRegistryName, xmlResources.ExternalHostsTarget);
            // InternalHostsTarget
            RegistryUtility.FRegistrySetValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrInternalHostsTargetRegistryName, xmlResources.InternalHostsTarget);
            // ApplyInExternalHosts (Enabled)

            RegistryUtility.FRegistrySetDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrIsApplyInExternalHostsRegistryName, (uint)xmlResources.ApplyInExternalHosts);
            // UpdateInternalHosts (Enabled)
            RegistryUtility.FRegistrySetDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrIsUpdateInternalHostsRegistryName, (uint)xmlResources.UpdateInternalHosts);
            // NetworkAuthentication (Disabled)
            RegistryUtility.FRegistrySetDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrIsNetworkAuthenticationName, (uint)xmlResources.NetworkAuthentication);
            // NetworkAuthenticationUsername (Empty)
            RegistryUtility.FRegistrySetValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrNetworkAuthenticationUsernameName, xmlResources.NetworkUsername);
            // NetworkAuthenticationPassword (Empty)
            RegistryUtility.FRegistrySetBinaryValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrNetworkAuthenticationPasswordName, xmlResources.NetworkPassword);
        }

        /// <summary>
        /// 必須項目の確認
        /// </summary>
        /// <returns></returns>
        private bool CheckEnoughRequiredValue()
        {
            if (!IsEnoughRequiredInternalHosts) return false;
            if (!IsEnoughRequiredExternalHosts) return false;
            return true;
        }

        /// <summary>
        /// タイマー実行される関数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {

            if (serviceController != null)
            {
                serviceController.Dispose();
            }
            serviceController = new ServiceController("AutoHostsUpdateService");

            switch (serviceController.Status)
            {
                case ServiceControllerStatus.Stopped:
                    // サービスは実行されていません
                    serviceStatusStatusBarItem.Content = Properties.Resources.StatusStopped;
                    serviceStartStopButton.Content = Properties.Resources.ServiceStartButton;
                    serviceStartStopButton.IsEnabled = true;
                    break;
                case ServiceControllerStatus.StartPending:
                    // サービスは開始中です
                    serviceStatusStatusBarItem.Content = Properties.Resources.StatusStartPending;
                    break;
                case ServiceControllerStatus.StopPending:
                    // サービスは停止中です
                    serviceStatusStatusBarItem.Content = Properties.Resources.StatusStopPending;
                    break;
                case ServiceControllerStatus.Running:
                    // サービスは実行中です
                    serviceStatusStatusBarItem.Content = Properties.Resources.StatusRunning;
                    serviceStartStopButton.Content = Properties.Resources.ServiceStopButton;
                    serviceStartStopButton.IsEnabled = true;
                    break;
                case ServiceControllerStatus.ContinuePending:
                    // サービスの継続は保留中です
                    serviceStatusStatusBarItem.Content = Properties.Resources.StatusContinuePending;
                    break;
                case ServiceControllerStatus.PausePending:
                    // サービスは実行されていません
                    serviceStatusStatusBarItem.Content = Properties.Resources.StatusStopped;
                    break;
                case ServiceControllerStatus.Paused:
                    // サービスの一時中断は保留中です
                    serviceStatusStatusBarItem.Content = Properties.Resources.StatusPausePending;
                    break;
                default:
                    // それ以外の状態
                    serviceStatusStatusBarItem.Content = Properties.Resources.StatusUndefined;
                    serviceStartStopButton.IsEnabled = false;
                    break;
            }
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
                    if(serviceController != null)
                    {
                        serviceController.Dispose();
                    }
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~MainWindow() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
