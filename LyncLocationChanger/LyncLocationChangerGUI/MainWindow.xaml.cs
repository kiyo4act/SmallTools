using Microsoft.Lync.Model;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.Storage;
using Windows.Storage.Streams;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.Win32;
using Windows.System;

namespace LyncLocationChangerGUI
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static LyncClient _lyncClient;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private string currentLocationName;
        private string readFileName = "MagellanLocation.csv";

        public MainWindow()
        {
            InitializeComponent();

            Microsoft.Win32.SystemEvents.PowerModeChanged += new Microsoft.Win32.PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock || e.Reason == SessionSwitchReason.SessionLogoff || e.Reason == SessionSwitchReason.RemoteConnect)
            {
                dispatcherTimer.Stop();
                ChangeLyncLocation("");
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock || e.Reason == SessionSwitchReason.SessionLogon)
            {
                dispatcherTimer.Start();
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                dispatcherTimer.Stop();
                ChangeLyncLocation("");
            }
            else if(e.Mode == PowerModes.Resume)
            {
                dispatcherTimer.Start();
            }
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            if (takeLyncInstance())
            {
                string newLocation = LocationText.Text.Length == 0 ? null : LocationText.Text;
                ChangeLyncLocationManually(newLocation);
                LocationText.Text = "";
            }
        }

        private bool takeLyncInstance()
        {
            try
            {
                _lyncClient = LyncClient.GetClient();
                if (textBlockLyncId.Text != _lyncClient.SignInConfiguration.UserName) textBlockLyncId.Text = _lyncClient.SignInConfiguration.UserName;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool ChangeLyncLocation(string newLocation)
        {
            if (currentLocationName != newLocation)
            {
                Dictionary<PublishableContactInformationType, object> data =
                                new Dictionary<PublishableContactInformationType, object>
                                {
                    {PublishableContactInformationType.LocationName, newLocation}
                                };

                try
                {
                    _lyncClient.Self.BeginPublishContactInformation(data, PublishInformationCallback, null);
                    currentLocationName = newLocation;
                    Trace.WriteLine(string.Format("Change Location: {0}", currentLocationName));
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        private async void ChangeLyncLocationManually(string newLocation)
        {
            bool fResult = await DeletePlaceFile(readFileName);
            if (fResult) ChangeLyncLocation(newLocation);
        }

        private async Task<bool> DeletePlaceFile(string readFileName)
        {
            try
            {
                StorageFolder folder = KnownFolders.PicturesLibrary;
                StorageFile file = await folder.CreateFileAsync(readFileName, CreationCollisionOption.ReplaceExisting);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
            
        }

        async void dispatcherTimer_Tick(object sender, object e)
        {
            if (takeLyncInstance())
            {               
                try
                {
                    StorageFolder folder = KnownFolders.PicturesLibrary;
                    StorageFile file = await folder.GetFileAsync(readFileName);
                    using (IRandomAccessStream rStream = await file.OpenAsync(FileAccessMode.Read))
                    using (StreamReader reader = new StreamReader(rStream.AsStream(), Encoding.UTF8))
                    {
                        ChangeLyncLocation(reader.ReadLine());

                    }
                }
                catch(Exception)
                {
                    ChangeLyncLocation("");
                }
            }
        }

        private static void PublishInformationCallback(IAsyncResult result)
        {
            _lyncClient.Self.EndPublishContactInformation(result);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // クローズ処理をキャンセルして、タスクバーの表示も消す
            e.Cancel = true;
            this.WindowState = System.Windows.WindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Minimized) this.ShowInTaskbar = false;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // チェックボックスの初期値を設定から適用
            Properties.Settings.Default.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Default_PropertyChanged);

            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            takeLyncInstance();
            initUser();
            Trace.Listeners.Add(new TextBoxTraceListener(textBoxLog));
        }
        /// <summary>
        /// アプリケーションプロパティの値が変更された後に発生するイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 変更されたらすぐに保存する
            Properties.Settings.Default.Save();
        }
        private void textBoxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxLog.ScrollToEnd();
        }
        private void initUser()
        {
            var hostNames = Windows.Networking.Connectivity.NetworkInformation.GetHostNames();
            textBlockPcName.Text = hostNames.First().CanonicalName.ToLower();
        }

        private void StartActivity()
        {
            Trace.WriteLine("Start dispatcher");
            if (Properties.Settings.Default.MinimizeStart) this.WindowState = System.Windows.WindowState.Minimized;
            buttonStart.IsEnabled = false;
            buttonStop.IsEnabled = true;
            dispatcherTimer.Start();
        }
        public void QuitActivity()
        {
            if (dispatcherTimer.IsEnabled) dispatcherTimer.Stop();
            System.Windows.Application.Current.Shutdown();
            Properties.Settings.Default.Save();
        }
        private async Task<bool> LaunchMagellan()
        {
            return await Launcher.LaunchUriAsync(new Uri("magellan-ble://"));
        }
        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            StartActivity();
        }
        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Stop dispatcher");
            if (dispatcherTimer.IsEnabled) dispatcherTimer.Stop();
            buttonStop.IsEnabled = false;
            buttonStart.IsEnabled = true;
        }
        private void buttonQuit_Click(object sender, RoutedEventArgs e)
        {
            QuitActivity();
        }
        private async void buttonMagellan_Click(object sender, RoutedEventArgs e)
        {
            bool result = await LaunchMagellan();
        }
        private void checkBoxAutoStart_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoStart = checkBoxAutoStart.IsChecked.GetValueOrDefault();
        }

        private void checkBoxMinimize_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.MinimizeStart = checkBoxAutoStart.IsChecked.GetValueOrDefault();
        }
        private void checkBoxMagellan_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.StartupMagellan = checkBoxMagellan.IsChecked.GetValueOrDefault();
        }
        private void checkBoxAutoStart_Loaded(object sender, RoutedEventArgs e)
        {
            checkBoxAutoStart.IsChecked = Properties.Settings.Default.AutoStart;
            if (Properties.Settings.Default.AutoStart) StartActivity();
        }
        private void checkBoxMinimize_Loaded(object sender, RoutedEventArgs e)
        {
            checkBoxMinimize.IsChecked = Properties.Settings.Default.MinimizeStart;
        }
        private async void checkBoxMagellan_Loaded(object sender, RoutedEventArgs e)
        {
            checkBoxMagellan.IsChecked = Properties.Settings.Default.StartupMagellan;
            bool result;
            if (Properties.Settings.Default.StartupMagellan) result = await LaunchMagellan();
        }
    }
}
