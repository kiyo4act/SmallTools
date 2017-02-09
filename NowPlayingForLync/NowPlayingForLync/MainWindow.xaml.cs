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
using System.Windows.Threading;
using mshtml;
using Microsoft.Lync.Model;
using Microsoft.Win32;

namespace NowPlayingForLync
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private string _currentPlayTitle;
        private string _currentPlayArtist;
        private LyncClient _lyncClient;
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                _lyncClient = LyncClient.GetClient();
            }
            catch (Exception)
            {
                this.Close();
            }
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += TimerOnTick;

            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }
        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock || e.Reason == SessionSwitchReason.RemoteConnect)
            {
                StopMusic();
                _timer.Stop();
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                _timer.Start();
            }
        }
        internal void QuitActivity()
        {
            if(_timer.IsEnabled) _timer.Stop();
            UpdateLyncPersonalNote("");
            System.Windows.Application.Current.Shutdown();
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            if (Browser.IsLoaded)
            {
                var document = (Browser.Document as HTMLDocument);
                if (document != null)
                {
                    var playStatus = document.getElementById("player-bar-play-pause");
                    if (playStatus != null)
                    {
                        if (playStatus.className == "x-scope paper-icon-button-0")
                        {
                            // 停止中
                            _currentPlayArtist = string.Empty;
                            _currentPlayTitle = string.Empty;
                            UpdateLyncPersonalNote("");
                        }
                        else if (playStatus.className == "x-scope paper-icon-button-0 playing")
                        {
                            // 再生中
                            var playTitle = document.getElementById("currently-playing-title");
                            var playArtist = document.getElementById("player-artist");
                            if (playTitle != null && playArtist != null)
                            {
                                if (_currentPlayTitle != playTitle.innerText || _currentPlayArtist != playArtist.innerText)
                                {
                                    _currentPlayTitle = playTitle.innerText;
                                    _currentPlayArtist = playArtist.innerText;
                                    UpdateLyncPersonalNote("🎧 Now Playing: "+_currentPlayTitle +" - "+ _currentPlayArtist);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void StartMusic()
        {
            if (Browser.IsLoaded)
            {
                var document = (Browser.Document as HTMLDocument);
                if (document != null)
                {
                    var playStatus = document.getElementById("player-bar-play-pause");
                    if (playStatus != null)
                    {
                        if (playStatus.className == "x-scope paper-icon-button-0")
                        {
                            playStatus.click();
                        }
                    }
                }
            }
        }
        public void StopMusic()
        {
            if (Browser.IsLoaded)
            {
                var document = (Browser.Document as HTMLDocument);
                if (document != null)
                {
                    var playStatus = document.getElementById("player-bar-play-pause");
                    if (playStatus != null)
                    {
                        if (playStatus.className == "x-scope paper-icon-button-0 playing")
                        {
                            playStatus.click();
                        }
                    }
                }
            }
        }
        private bool UpdateLyncPersonalNote(string note)
        {
            if (_lyncClient != null)
            {
                Dictionary<PublishableContactInformationType, object> data =
                                new Dictionary<PublishableContactInformationType, object>
                                {
                    {PublishableContactInformationType.PersonalNote, note}
                                };

                try
                {
                    _lyncClient.Self.BeginPublishContactInformation(data, PublishInformationCallback, null);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }
        private void PublishInformationCallback(IAsyncResult result)
        {
            _lyncClient.Self.EndPublishContactInformation(result);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Browser.Navigate("https://play.google.com/music/");
            _timer.Start();
        }

        private void Browser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            var document = Browser.Document as HTMLDocument;
            if (document != null) Title = document.title;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            QuitActivity();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Minimized) this.ShowInTaskbar = false;
        }
    }
}
