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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;
using Utilities;
using System.Configuration;
using System.ComponentModel;
using System.Windows.Threading;
using Form = System.Windows.Forms;
using System.IO;

namespace AutoTapGUI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private string presetPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\LWR\AutoTap\Preset\";
        private string logPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\LWR\AutoTap\Log\";
        private CancellationTokenSource _tokenSource = null;
        private string[] presetModesArray = { "Zombie", "Dominate","Tough", "Defend", "Recon" };
        private string[] presetNextModesArray = { "None", "Dominate","Tough", "Zombie" };
        private string[] radarOrderArray = { "0", "1", "2", "3", "4", "5", "6", "7" };
        private TextBoxTraceListener _listener;
        DispatcherTimer dispatcherStopTimer;
        //private bool fStarted = false;

        public MainWindow()
        {
            InitializeComponent();

            dispatcherStopTimer = new DispatcherTimer(DispatcherPriority.Normal);
            dispatcherStopTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherStopTimer.Tick += new EventHandler(dispatcherTimer_Tick);

#if DEBUG
            presetPath = @"Preset\";
#endif
        }

        /// <summary>
        /// タイマー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now >= (wfhStop.Child as Form.DateTimePicker).Value)
            {
                dispatcherStopTimer.Stop();
                if (_tokenSource != null) _tokenSource.Cancel(true);
                buttonStop.IsEnabled = false;
                buttonStop.Content = "Stopping...";
                Trace.WriteLine("---------- 終了タイマーによる停止要求が発生しました ----------");
            }
        }

        /// <summary>
        /// 「Start」ボタンがクリックされたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            // 開始タイマーがONの場合は未来が指定されているかチェック
            if (isStartTimer.IsChecked.GetValueOrDefault() && DateTime.Now >= (wfhStart.Child as Form.DateTimePicker).Value)
            {
                MessageBox.Show("開始タイマーは今より未来の時間を指定してください。", "開始タイマーエラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // 終了タイマーがONの場合は未来が指定されているかチェック
            else if (isStopTimer.IsChecked.GetValueOrDefault() && DateTime.Now >= (wfhStop.Child as Form.DateTimePicker).Value)
            {
                MessageBox.Show("終了タイマーは今より未来の時間を指定してください。", "終了タイマーエラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // 開始・終了タイマーがONの場合は開始タイマーより未来が指定されているかチェック
            else if (isStartTimer.IsChecked.GetValueOrDefault() && isStopTimer.IsChecked.GetValueOrDefault() && (wfhStart.Child as Form.DateTimePicker).Value >= (wfhStop.Child as Form.DateTimePicker).Value)
            {
                MessageBox.Show("終了タイマーは開始タイマーより未来の時間を指定してください。", "終了タイマーエラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                buttonStart.IsEnabled = false;
                buttonStop.IsEnabled = true;
                //fStarted = true;

                // 「開始時に最小化」のチェックボックスがONなら最小化する
                if (isStartMinimize.IsChecked.GetValueOrDefault())
                {
                    this.WindowState = System.Windows.WindowState.Minimized;
                    this.ShowInTaskbar = false;
                }

                // WorkingModeのアイテムをEnumに変換する
                if (Enum.IsDefined(typeof(StationMemoryOperation.WorkingMode), comboBoxWorkingMode.SelectedItem.ToString()))
                {
                    StationMemoryOperation.WorkingMode workingMode = (StationMemoryOperation.WorkingMode)Enum.Parse(
                        typeof(StationMemoryOperation.WorkingMode),
                        comboBoxWorkingMode.SelectedItem.ToString()
                        );
                    StationMemoryOperation.WorkingMode nextMode = (StationMemoryOperation.WorkingMode)Enum.Parse(
                        typeof(StationMemoryOperation.WorkingMode),
                        comboBoxNextMode.SelectedItem.ToString()
                        );
                    bool[] isUseBatteryArray = { Properties.Settings.Default.UseBattery50, Properties.Settings.Default.UseBattery150, Properties.Settings.Default.UseBattery300 };
                    int radarOrder = Properties.Settings.Default.SelectRadarOrder;
                    bool sleepLog = Properties.Settings.Default.ShowSleepDetail;
                    IAutomateOperation stationMemoryOperation = new StationMemoryOperation(presetPath + comboBoxSelectConfig.SelectedItem.ToString(), workingMode, isUseBatteryArray, nextMode, radarOrder, sleepLog);
                    // CancellationTokenを初期化
                    if (_tokenSource == null) _tokenSource = new CancellationTokenSource();

                    // 開始タイマーチェックボックスがONなら開始タイマーを起動する
                    if (isStartTimer.IsChecked.GetValueOrDefault())
                    {
                        DateTime startTime = (wfhStart.Child as Form.DateTimePicker).Value;
                        Trace.WriteLine(string.Format("開始タイマーの時間: {0}", startTime));

                        await Task.Run(() => {
                            while (DateTime.Now < startTime && !_tokenSource.Token.IsCancellationRequested)
                            {
                                Thread.Sleep(1000);
                            }
                            Trace.WriteLine("---------- 開始タイマーによる開始要求が発生しました ----------");
                        });
                        // 開始タイマーをやめる
                        isStartTimer.IsChecked = false;
                    }

                    // 終了タイマーチェックボックスがONなら終了タイマーを起動する
                    if (isStopTimer.IsChecked.GetValueOrDefault())
                    {
                        dispatcherStopTimer.Start();
                        Trace.WriteLine(string.Format("終了タイマーの時間: {0}", (wfhStop.Child as Form.DateTimePicker).Value));
                    }

                    await stationMemoryOperation.DoAutomateTaskAsync(_tokenSource.Token).ContinueWith(t =>
                    {
                        // TODO:あとしまつ
                        _tokenSource.Dispose();
                        _tokenSource = null;

                        if (t.IsCanceled)
                        {
                            Trace.WriteLine("---------- 自動処理はキャンセルされました ----------");
                        }
                        else
                        {
                            Trace.WriteLine("---------- 自動処理は停止しました ----------");
                        }
                    });
                }

                // 終了タイマーチェックボックスがONかつタイマー動作中ならキャンセルと判断
                if (isStopTimer.IsChecked.GetValueOrDefault() && dispatcherStopTimer.IsEnabled)
                {
                    dispatcherStopTimer.Stop();
                }

                // 終了タイマーチェックボックスがONかつタイマー非動作中ならタイマー終了と判断
                if (isStopTimer.IsChecked.GetValueOrDefault() && !dispatcherStopTimer.IsEnabled)
                {
                    // 終了タイマーをやめる
                    isStopTimer.IsChecked = false;
                }

                buttonStop.Content = "Stop";
                //fStarted = false;
                buttonStop.IsEnabled = false;
                buttonStart.IsEnabled = true;
            }
        }
        /// <summary>
        /// 「Stop」ボタンがクリックされたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            if (isStopTimer.IsChecked.GetValueOrDefault()) dispatcherStopTimer.Stop();
            if (_tokenSource != null) _tokenSource.Cancel(true);
            buttonStop.IsEnabled = false;
            buttonStop.Content = "Stopping...";
            Trace.WriteLine("---------- 停止ボタンによる停止要求が発生しました ----------");
        }
        /// <summary>
        /// 「Quit」ボタンがクリックされたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonQuit_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName("adb");

            foreach (System.Diagnostics.Process p in ps)
            {
                //プロセスを強制的に終了させる
                p.Kill();
            }
            System.Windows.Application.Current.Shutdown();
        }
        /// <summary>
        /// 「Save」ボタンがクリックされたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            SavePreset(Properties.Settings.Default.SelectedPresetFile);
            buttonSave.IsEnabled = false;
        }
        /// <summary>
        /// ウィンドウの閉じるボタンがクリックされたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // クローズ処理をキャンセルして、タスクバーの表示も消す
            e.Cancel = true;
            this.WindowState = System.Windows.WindowState.Minimized;
            this.ShowInTaskbar = false;
        }
        /// <summary>
        /// ウィンドウの状態が変化したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Minimized) this.ShowInTaskbar = false;
        }
        /// <summary>
        /// ウィンドウがロードされたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Init()) System.Windows.Application.Current.Shutdown();
            // 設定変更時のイベントを追加
            
        }
        /// <summary>
        /// 初期化メソッド
        /// </summary>
        /// <returns>例外なく初期化できたか</returns>
        private bool Init()
        {
            try
            {
                Directory.CreateDirectory(presetPath);
                // Presetをコピー
                foreach (string stFilePath in System.IO.Directory.GetFiles(@"Preset\", "*.ini"))
                {
                    File.Copy(stFilePath, presetPath + System.IO.Path.GetFileName(stFilePath));
                }
            }
            catch (Exception)
            {

            }

            try
            {
                // Preset一覧をロード
                foreach (string stFilePath in Directory.GetFiles(presetPath, "*.ini"))
                {
                    comboBoxSelectConfig.Items.Add(System.IO.Path.GetFileName(stFilePath));
                }

                foreach (string mode in presetModesArray)
                {
                    comboBoxWorkingMode.Items.Add(mode);
                }

                foreach (string mode in presetNextModesArray)
                {
                    comboBoxNextMode.Items.Add(mode);
                }
                foreach (string mode in radarOrderArray)
                {
                    comboBoxSelectRadarOrder.Items.Add(mode);
                }
                // コンボボックスの初期値を設定から適用
                //comboBoxSelectConfig.IndexOf(Properties.Settings.Default.SelectedPresetFile);
                comboBoxSelectConfig.Text = Properties.Settings.Default.SelectedPresetFile;
                comboBoxWorkingMode.Text = Properties.Settings.Default.SelectedWorkingMode;
                comboBoxNextMode.Text = Properties.Settings.Default.SelectedNextMode;
                comboBoxSelectRadarOrder.Text = Properties.Settings.Default.SelectRadarOrder.ToString();
                // チェックボックスの初期値を設定から適用
                Properties.Settings.Default.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Default_PropertyChanged);
                // 「Stop」ボタンを無効化
                buttonStop.IsEnabled = false;

                // TraceListenerをロード
                Directory.CreateDirectory(logPath);
                string logFilePath = string.Format("{0}{1}.log", logPath, DateTime.Now.ToString("yyyyMMddHHmmss"));
                _listener = new TextBoxTraceListener(textBoxLog,logFilePath);
                // 「ログ」テキストボックスにTraceを適用
                Trace.Listeners.Add(_listener);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
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
        /// <summary>
        /// 「ログ」テキストボックス内が変更されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxLog.ScrollToEnd();
            if (textBoxLog.LineCount >= 1000)
            {
                //textBoxLog.Clear();
                TextBoxClearDelegate InvokeClear = new TextBoxClearDelegate(ClearTextBox);
                textBoxLog.Dispatcher.Invoke(InvokeClear, new Object[] { textBoxLog });
            }
        }

        private delegate void TextBoxClearDelegate(TextBox target);

        private void ClearTextBox(TextBox target)
        {
            target.Clear();
        }

        /// <summary>
        /// 「開始時に最小化」チェックボックスが変更されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isStartMinimize_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.StartMinimize = isStartMinimize.IsChecked.GetValueOrDefault();
        }
        /// <summary>
        /// 「詳細なスリープをログに表示」チェックボックスが変更されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isShowSleepDetail_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ShowSleepDetail = isShowSleepDetail.IsChecked.GetValueOrDefault();
        }
        /// <summary>
        /// 「ログをファイルに出力」チェックボックスが変更されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isOutputLog_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.OutputLog = isOutputLog.IsChecked.GetValueOrDefault();
            if (_listener != null) _listener.IsOutput = Properties.Settings.Default.OutputLog;
        }
        /// <summary>
        /// 「開始タイマー」チェックボックスが変更されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isStartTimer_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.StartTimer = isStartTimer.IsChecked.GetValueOrDefault();
            wfhStart.IsEnabled = Properties.Settings.Default.StartTimer;
        }
        /// <summary>
        /// 「終了タイマー」チェックボックスが変更されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isStopTimer_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.StopTimer = isStopTimer.IsChecked.GetValueOrDefault();
            wfhStop.IsEnabled = Properties.Settings.Default.StopTimer;
        }
        /// <summary>
        /// 「50」チェックボックスが変更されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isUseBattery50_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UseBattery50 = isUseBattery50.IsChecked.GetValueOrDefault();
            // 150も300も無効の場合は50を強制ONで操作不可能に
            if (!Properties.Settings.Default.UseBattery150 && !Properties.Settings.Default.UseBattery300)
            {
                isUseBattery50.IsEnabled = false;
                isUseBattery50.IsChecked = true;
            }
            else
            {
                isUseBattery50.IsEnabled = true;
            }
        }
        /// <summary>
        /// 「150」チェックボックスが変更されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isUseBattery150_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UseBattery150 = isUseBattery150.IsChecked.GetValueOrDefault();
            // 150も300も無効の場合は50を強制ONで操作不可能に
            if (!Properties.Settings.Default.UseBattery150 && !Properties.Settings.Default.UseBattery300)
            {
                isUseBattery50.IsEnabled = false;
                isUseBattery50.IsChecked = true;
            }
            else
            {
                isUseBattery50.IsEnabled = true;
            }
        }
        /// <summary>
        /// 「300」チェックボックスが変更されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isUseBattery300_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UseBattery300 = isUseBattery300.IsChecked.GetValueOrDefault();
            // 150も300も無効の場合は50を強制ONで操作不可能に
            if (!Properties.Settings.Default.UseBattery150 && !Properties.Settings.Default.UseBattery300)
            {
                isUseBattery50.IsEnabled = false;
                isUseBattery50.IsChecked = true;
            }
            else
            {
                isUseBattery50.IsEnabled = true;
            }
        }
        /// <summary>
        /// 「開始時に最小化」チェックボックスがロードされたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isStartMinimize_Loaded(object sender, RoutedEventArgs e)
        {
            // プロパティの設定を適用する
            isStartMinimize.IsChecked = Properties.Settings.Default.StartMinimize;
        }
        /// <summary>
        /// 「詳細なスリープをログに表示」チェックボックスがロードされたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isShowSleepDetail_Loaded(object sender, RoutedEventArgs e)
        {
            // プロパティの設定を適用する
            isShowSleepDetail.IsChecked = Properties.Settings.Default.ShowSleepDetail;
        }
        /// <summary>
        /// 「ログをファイルに出力」チェックボックスがロードされたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isOutputLog_Loaded(object sender, RoutedEventArgs e)
        {
            // プロパティの設定を適用する
            isOutputLog.IsChecked = Properties.Settings.Default.OutputLog;
            if (_listener != null) _listener.IsOutput = Properties.Settings.Default.OutputLog;
        }
        /// <summary>
        /// 「開始タイマー」チェックボックスがロードされたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isStartTimer_Loaded(object sender, RoutedEventArgs e)
        {
            // プロパティの設定を適用する
            isStartTimer.IsChecked = Properties.Settings.Default.StartTimer;
            wfhStart.IsEnabled = Properties.Settings.Default.StartTimer;
        }
        /// <summary>
        /// 「終了タイマー」チェックボックスがロードされたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isStopTimer_Loaded(object sender, RoutedEventArgs e)
        {
            // プロパティの設定を適用する
            isStopTimer.IsChecked = Properties.Settings.Default.StopTimer;
            wfhStop.IsEnabled = Properties.Settings.Default.StopTimer;
        }

        /// <summary>
        /// 「50」チェックボックスがロードされたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isUseBattery50_Loaded(object sender, RoutedEventArgs e)
        {
            // プロパティの設定を適用する
            isUseBattery50.IsChecked = Properties.Settings.Default.UseBattery50;
        }

        /// <summary>
        /// 「150」チェックボックスがロードされたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isUseBattery150_Loaded(object sender, RoutedEventArgs e)
        {
            // プロパティの設定を適用する
            isUseBattery150.IsChecked = Properties.Settings.Default.UseBattery150;
        }

        /// <summary>
        /// 「300」チェックボックスがロードされたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isUseBattery300_Loaded(object sender, RoutedEventArgs e)
        {
            // プロパティの設定を適用する
            isUseBattery300.IsChecked = Properties.Settings.Default.UseBattery300;
        }
        /// <summary>
        /// 「Config」コンボボックスの選択を変更したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxSelectConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.SelectedPresetFile = comboBoxSelectConfig.SelectedItem.ToString();
            ExtractPreset(comboBoxSelectConfig.SelectedItem.ToString());
            buttonSave.IsEnabled = false;
        }
        /// <summary>
        /// 「Working Mode」コンボボックスの選択を変更したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxWorkingMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.SelectedWorkingMode = comboBoxWorkingMode.SelectedItem.ToString();
            if (comboBoxWorkingMode.SelectedItem.ToString() == presetModesArray[3] || comboBoxWorkingMode.SelectedItem.ToString() == presetModesArray[4])
            {
                comboBoxNextMode.IsEnabled = true;
            }
            else
            {
                comboBoxNextMode.IsEnabled = false;
            }
        }
        /// <summary>
        /// 「Next Mode」コンボボックスの選択を変更したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxNextMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.SelectedNextMode = comboBoxNextMode.SelectedItem.ToString();
        }
        /// <summary>
        /// 「使用するレーダー」コンボボックスの選択を変更したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxSelectRadarOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.SelectRadarOrder = int.Parse(comboBoxSelectRadarOrder.SelectedItem.ToString());
        }
        /// <summary>
        /// プリセット一覧をConfig画面に展開する
        /// </summary>
        /// <param name="presetFileName"></param>
        private void ExtractPreset(string presetFileName)
        {
            InifileUtils ini = new InifileUtils(presetPath + presetFileName);

            // Area
            // Check In button area
            CheckInAreaXStart.Text = ini.getValueString("Area", "CheckInAreaXStart");
            CheckInAreaYStart.Text = ini.getValueString("Area", "CheckInAreaYStart");
            CheckInAreaXEnd.Text = ini.getValueString("Area", "CheckInAreaXEnd");
            CheckInAreaYEnd.Text = ini.getValueString("Area", "CheckInAreaYEnd");

            // Link button area
            LinkAreaXStart.Text = ini.getValueString("Area", "LinkAreaXStart");
            LinkAreaYStart.Text = ini.getValueString("Area", "LinkAreaYStart");
            LinkAreaXEnd.Text = ini.getValueString("Area", "LinkAreaXEnd");
            LinkAreaYEnd.Text = ini.getValueString("Area", "LinkAreaYEnd");

            // Window Close area
            WindowCloseAreaXStart.Text = ini.getValueString("Area", "WindowCloseAreaXStart");
            WindowCloseAreaYStart.Text = ini.getValueString("Area", "WindowCloseAreaYStart");
            WindowCloseAreaXEnd.Text = ini.getValueString("Area", "WindowCloseAreaXEnd");
            WindowCloseAreaYEnd.Text = ini.getValueString("Area", "WindowCloseAreaYEnd");

            // Bag Button area
            BagButtonAreaXStart.Text = ini.getValueString("Area", "BagButtonAreaXStart");
            BagButtonAreaYStart.Text = ini.getValueString("Area", "BagButtonAreaYStart");
            BagButtonAreaXEnd.Text = ini.getValueString("Area", "BagButtonAreaXEnd");
            BagButtonAreaYEnd.Text = ini.getValueString("Area", "BagButtonAreaYEnd");

            // Battery Button area
            BatteryButtonAreaXStart.Text = ini.getValueString("Area", "BatteryButtonAreaXStart");
            BatteryButtonAreaYStart.Text = ini.getValueString("Area", "BatteryButtonAreaYStart");
            BatteryButtonAreaXEnd.Text = ini.getValueString("Area", "BatteryButtonAreaXEnd");
            BatteryButtonAreaYEnd.Text = ini.getValueString("Area", "BatteryButtonAreaYEnd");

            // Charge Button area
            Charge50ButtonAreaXStart.Text = ini.getValueString("Area", "Charge50ButtonAreaXStart");
            Charge50ButtonAreaYStart.Text = ini.getValueString("Area", "Charge50ButtonAreaYStart");
            Charge50ButtonAreaXEnd.Text = ini.getValueString("Area", "Charge50ButtonAreaXEnd");
            Charge50ButtonAreaYEnd.Text = ini.getValueString("Area", "Charge50ButtonAreaYEnd");

            // Charge Button area
            Charge150ButtonAreaXStart.Text = ini.getValueString("Area", "Charge150ButtonAreaXStart");
            Charge150ButtonAreaYStart.Text = ini.getValueString("Area", "Charge150ButtonAreaYStart");
            Charge150ButtonAreaXEnd.Text = ini.getValueString("Area", "Charge150ButtonAreaXEnd");
            Charge150ButtonAreaYEnd.Text = ini.getValueString("Area", "Charge150ButtonAreaYEnd");

            // Charge Button area
            Charge300ButtonAreaXStart.Text = ini.getValueString("Area", "Charge300ButtonAreaXStart");
            Charge300ButtonAreaYStart.Text = ini.getValueString("Area", "Charge300ButtonAreaYStart");
            Charge300ButtonAreaXEnd.Text = ini.getValueString("Area", "Charge300ButtonAreaXEnd");
            Charge300ButtonAreaYEnd.Text = ini.getValueString("Area", "Charge300ButtonAreaYEnd");

            // Close Button area
            CloseButtonAreaXStart.Text = ini.getValueString("Area", "CloseButtonAreaXStart");
            CloseButtonAreaYStart.Text = ini.getValueString("Area", "CloseButtonAreaYStart");
            CloseButtonAreaXEnd.Text = ini.getValueString("Area", "CloseButtonAreaXEnd");
            CloseButtonAreaYEnd.Text = ini.getValueString("Area", "CloseButtonAreaYEnd");

            // Error Button area
            ErrorButtonAreaXStart.Text = ini.getValueString("Area", "ErrorButtonAreaXStart");
            ErrorButtonAreaYStart.Text = ini.getValueString("Area", "ErrorButtonAreaYStart");
            ErrorButtonAreaXEnd.Text = ini.getValueString("Area", "ErrorButtonAreaXEnd");
            ErrorButtonAreaYEnd.Text = ini.getValueString("Area", "ErrorButtonAreaYEnd");

            // Radar Button area
            RadarButtonAreaXStart.Text = ini.getValueString("Area", "RadarButtonAreaXStart");
            RadarButtonAreaYStart.Text = ini.getValueString("Area", "RadarButtonAreaYStart");
            RadarButtonAreaXEnd.Text = ini.getValueString("Area", "RadarButtonAreaXEnd");
            RadarButtonAreaYEnd.Text = ini.getValueString("Area", "RadarButtonAreaYEnd");

            // Radar Button area
            Radar1ButtonAreaXStart.Text = ini.getValueString("Area", "Radar1ButtonAreaXStart");
            Radar1ButtonAreaYStart.Text = ini.getValueString("Area", "Radar1ButtonAreaYStart");
            Radar1ButtonAreaXEnd.Text = ini.getValueString("Area", "Radar1ButtonAreaXEnd");
            Radar1ButtonAreaYEnd.Text = ini.getValueString("Area", "Radar1ButtonAreaYEnd");
            RadarButtonsOffsetY.Text = ini.getValueString("Area", "RadarButtonsOffsetY");

            // Radar Close Button area
            RadarCloseButtonAreaXStart.Text = ini.getValueString("Area", "RadarCloseButtonAreaXStart");
            RadarCloseButtonAreaYStart.Text = ini.getValueString("Area", "RadarCloseButtonAreaYStart");
            RadarCloseButtonAreaXEnd.Text = ini.getValueString("Area", "RadarCloseButtonAreaXEnd");
            RadarCloseButtonAreaYEnd.Text = ini.getValueString("Area", "RadarCloseButtonAreaYEnd");

            //;Return Maintenance Button area
            ReturnMaintenanceButtonAreaXStart.Text = ini.getValueString("Area", "ReturnMaintenanceButtonAreaXStart");
            ReturnMaintenanceButtonAreaYStart.Text = ini.getValueString("Area", "ReturnMaintenanceButtonAreaYStart");
            ReturnMaintenanceButtonAreaXEnd.Text = ini.getValueString("Area", "ReturnMaintenanceButtonAreaXEnd");
            ReturnMaintenanceButtonAreaYEnd.Text = ini.getValueString("Area", "ReturnMaintenanceButtonAreaYEnd");

            // Interval
            // Window Close interval
            WindowOpenInterval.Text = ini.getValueString("Interval", "WindowOpenInterval");
            WindowCloseInterval.Text = ini.getValueString("Interval", "WindowCloseInterval");
            WindowCloseAnotherInterval.Text = ini.getValueString("Interval", "WindowCloseAnotherInterval");

            // Next Check In interval
            CheckInIntervalMin.Text = ini.getValueString("Interval", "CheckInIntervalMin");
            CheckInIntervalMax.Text = ini.getValueString("Interval", "CheckInIntervalMax");

            // Maintain Link interval
            MaintainLinkIntervalMin.Text = ini.getValueString("Interval", "MaintainLinkIntervalMin");
            MaintainLinkIntervalMax.Text = ini.getValueString("Interval", "MaintainLinkIntervalMax");

            // Charge Battely interval
            ChargeBattelyInterval.Text = ini.getValueString("Interval", "ChargeBattelyInterval");

            // Unlock Interval
            UnlockPowerInterval.Text = ini.getValueString("Interval", "UnlockPowerInterval");
            UnlockSwipeInterval.Text = ini.getValueString("Interval", "UnlockSwipeInterval");
            UnlockInputInterval.Text = ini.getValueString("Interval", "UnlockInputInterval");

            // Wakeup Interval
            WakeupInterval.Text = ini.getValueString("Interval", "WakeupInterval");

            // Close Button Interval
            CloseButtonInterval.Text = ini.getValueString("Interval", "CloseButtonInterval");

            // Error Button Interval
            ErrorButtonInterval.Text = ini.getValueString("Interval", "ErrorButtonInterval");

            // Charge Button Interval
            ChargeButtonInterval.Text = ini.getValueString("Interval", "ChargeButtonInterval");

            // Battry Button Interval
            BattryButtonInterval.Text = ini.getValueString("Interval", "BattryButtonInterval");

            // Bag Button Interval
            BagButtonInterval.Text = ini.getValueString("Interval", "BagButtonInterval");

            // CheckIn flow Interval
            DoCheckInInterval.Text = ini.getValueString("Interval", "DoCheckInInterval");
            CloseCheckInInterval.Text = ini.getValueString("Interval", "CloseCheckInInterval");

            // ScreenShot Interval
            TakeScreenShotInterval.Text = ini.getValueString("Interval", "TakeScreenShotInterval");
            PullFileInterval.Text = ini.getValueString("Interval", "PullFileInterval");
            RemoveFileInterval.Text = ini.getValueString("Interval", "RemoveFileInterval");

            // Radar Button Interval
            RadarButtonInterval.Text = ini.getValueString("Interval", "RadarButtonInterval");

            // Radar Check in Interval
            RadarCheckInInterval.Text = ini.getValueString("Interval", "RadarCheckInInterval");

            // Radar Close Button Interval
            RadarCloseButtonInterval.Text = ini.getValueString("Interval", "RadarCloseButtonInterval");

            // Radar Check in try
            RadarCheckInTryNum.Text = ini.getValueString("Interval", "RadarCheckInTryNum");
            RadarCheckInTryTime.Text = ini.getValueString("Interval", "RadarCheckInTryTime");
            RadarCheckInTryIntervalMin.Text = ini.getValueString("Interval", "RadarCheckInTryIntervalMin");
            RadarCheckInTryIntervalMax.Text = ini.getValueString("Interval", "RadarCheckInTryIntervalMax");

            // Radar Useable num
            RadarUseableNum.Text = ini.getValueString("Interval", "RadarUseableNum");

            // Tough Recovery Time
            ToughRecoveryTimeMin.Text = ini.getValueString("Interval", "ToughRecoveryTimeMin");
            ToughRecoveryTimeMax.Text = ini.getValueString("Interval", "ToughRecoveryTimeMax");

            // Tough Check Alive Interval
            ToughCheckAliveIntervalMin.Text = ini.getValueString("Interval", "ToughCheckAliveIntervalMin");
            ToughCheckAliveIntervalMax.Text = ini.getValueString("Interval", "ToughCheckAliveIntervalMax");

            // Image
            // ScreenShot Info
            ScreenShotTrimX.Text = ini.getValueString("Image", "ScreenShotTrimX");
            ScreenShotTrimY.Text = ini.getValueString("Image", "ScreenShotTrimY");
            ScreenShotTrimWidth.Text = ini.getValueString("Image", "ScreenShotTrimWidth");
            ScreenShotTrimHeight.Text = ini.getValueString("Image", "ScreenShotTrimHeight");

            // ScreenShot Path
            ScreenShotRemotePath.Text = ini.getValueString("Image", "ScreenShotRemotePath");
            ScreenShotLocalPath.Text = ini.getValueString("Image", "ScreenShotLocalPath");

            // OCR Info
            TrimImageLocalPath.Text = ini.getValueString("Image", "TrimImageLocalPath");
            OcrTextLocalPath.Text = ini.getValueString("Image", "OcrTextLocalPath");

            // Compared Image Path
            ComparedLinkImagePath.Text = ini.getValueString("Image", "ComparedLinkImagePath");
            ComparedErrorImagePath.Text = ini.getValueString("Image", "ComparedErrorImagePath");
            ComparedGreetingImagePath.Text = ini.getValueString("Image", "ComparedGreetingImagePath");
            ComparedLoadingImagePathA.Text = ini.getValueString("Image", "ComparedLoadingImagePathA");
            ComparedLoadingImagePathB.Text = ini.getValueString("Image", "ComparedLoadingImagePathB");
            ComparedMaintenanceImagePath.Text = ini.getValueString("Image", "ComparedMaintenanceImagePath");
            ComparedOpeningImagePath.Text = ini.getValueString("Image", "ComparedOpeningImagePath");
            ComparedTitleImagePath.Text = ini.getValueString("Image", "ComparedTitleImagePath");
            ComparedTimelineImagePath.Text = ini.getValueString("Image", "ComparedTimelineImagePath");

            // Threshold
            // Pattern matching threshold
            PatternMatchingThreshold.Text = ini.getValueString("Threshold", "PatternMatchingThreshold");

            // Battely threshold
            BatteryEnoughThreshold.Text = ini.getValueString("Threshold", "BatteryEnoughThreshold");
            BatteryMaxThreshold.Text = ini.getValueString("Threshold", "BatteryMaxThreshold");

            // No Recover threshold
            NoRecoverThreshold.Text = ini.getValueString("Threshold", "NoRecoverThreshold");

            // Unlock
            // Unlock
            UnlockType.Text = ini.getValueString("Unlock", "UnlockType");

            UnlockPassword.Password = ini.getValueString("Unlock", "UnlockPassword");
            UnlockXStart.Text = ini.getValueString("Unlock", "UnlockXStart");
            UnlockYStart.Text = ini.getValueString("Unlock", "UnlockYStart");
            UnlockXEnd.Text = ini.getValueString("Unlock", "UnlockXEnd");
            UnlockYEnd.Text = ini.getValueString("Unlock", "UnlockYEnd");
            UnlockDuration.Text = ini.getValueString("Unlock", "UnlockDuration");
        }

        /// <summary>
        /// Preset一覧をファイルに保存する
        /// </summary>
        /// <param name="presetFileName"></param>
        private void SavePreset(string presetFileName)
        {
            InifileUtils ini = new InifileUtils(presetPath + presetFileName);

            // Area
            // Check In button area
            ini.setValue("Area", "CheckInAreaXStart", CheckInAreaXStart.Text);
            ini.setValue("Area", "CheckInAreaYStart", CheckInAreaYStart.Text);
            ini.setValue("Area", "CheckInAreaXEnd", CheckInAreaXEnd.Text);
            ini.setValue("Area", "CheckInAreaYEnd", CheckInAreaYEnd.Text);

            // Link button area
            ini.setValue("Area", "LinkAreaXStart", LinkAreaXStart.Text);
            ini.setValue("Area", "LinkAreaYStart", LinkAreaYStart.Text);
            ini.setValue("Area", "LinkAreaXEnd", LinkAreaXEnd.Text);
            ini.setValue("Area", "LinkAreaYEnd", LinkAreaYEnd.Text);

            // Window Close area
            ini.setValue("Area", "WindowCloseAreaXStart", WindowCloseAreaXStart.Text);
            ini.setValue("Area", "WindowCloseAreaYStart", WindowCloseAreaYStart.Text);
            ini.setValue("Area", "WindowCloseAreaXEnd", WindowCloseAreaXEnd.Text);
            ini.setValue("Area", "WindowCloseAreaYEnd", WindowCloseAreaYEnd.Text);

            // Bag Button area
            ini.setValue("Area", "BagButtonAreaXStart", BagButtonAreaXStart.Text);
            ini.setValue("Area", "BagButtonAreaYStart", BagButtonAreaYStart.Text);
            ini.setValue("Area", "BagButtonAreaXEnd", BagButtonAreaXEnd.Text);
            ini.setValue("Area", "BagButtonAreaYEnd", BagButtonAreaYEnd.Text);

            // Battery Button area
            ini.setValue("Area", "BatteryButtonAreaXStart", BatteryButtonAreaXStart.Text);
            ini.setValue("Area", "BatteryButtonAreaYStart", BatteryButtonAreaYStart.Text);
            ini.setValue("Area", "BatteryButtonAreaXEnd", BatteryButtonAreaXEnd.Text);
            ini.setValue("Area", "BatteryButtonAreaYEnd", BatteryButtonAreaYEnd.Text);

            // Charge Button area
            ini.setValue("Area", "Charge50ButtonAreaXStart", Charge50ButtonAreaXStart.Text);
            ini.setValue("Area", "Charge50ButtonAreaYStart", Charge50ButtonAreaYStart.Text);
            ini.setValue("Area", "Charge50ButtonAreaXEnd", Charge50ButtonAreaXEnd.Text);
            ini.setValue("Area", "Charge50ButtonAreaYEnd", Charge50ButtonAreaYEnd.Text);

            // Charge Button area
            ini.setValue("Area", "Charge150ButtonAreaXStart", Charge150ButtonAreaXStart.Text);
            ini.setValue("Area", "Charge150ButtonAreaYStart", Charge150ButtonAreaYStart.Text);
            ini.setValue("Area", "Charge150ButtonAreaXEnd", Charge150ButtonAreaXEnd.Text);
            ini.setValue("Area", "Charge150ButtonAreaYEnd", Charge150ButtonAreaYEnd.Text);

            // Charge Button area
            ini.setValue("Area", "Charge300ButtonAreaXStart", Charge300ButtonAreaXStart.Text);
            ini.setValue("Area", "Charge300ButtonAreaYStart", Charge300ButtonAreaYStart.Text);
            ini.setValue("Area", "Charge300ButtonAreaXEnd", Charge300ButtonAreaXEnd.Text);
            ini.setValue("Area", "Charge300ButtonAreaYEnd", Charge300ButtonAreaYEnd.Text);

            // Close Button area
            ini.setValue("Area", "CloseButtonAreaXStart", CloseButtonAreaXStart.Text);
            ini.setValue("Area", "CloseButtonAreaYStart", CloseButtonAreaYStart.Text);
            ini.setValue("Area", "CloseButtonAreaXEnd", CloseButtonAreaXEnd.Text);
            ini.setValue("Area", "CloseButtonAreaYEnd", CloseButtonAreaYEnd.Text);

            // Error Button area
            ini.setValue("Area", "ErrorButtonAreaXStart", ErrorButtonAreaXStart.Text);
            ini.setValue("Area", "ErrorButtonAreaYStart", ErrorButtonAreaYStart.Text);
            ini.setValue("Area", "ErrorButtonAreaXEnd", ErrorButtonAreaXEnd.Text);
            ini.setValue("Area", "ErrorButtonAreaYEnd", ErrorButtonAreaYEnd.Text);

            // Radar Button area
            ini.setValue("Area", "RadarButtonAreaXStart", RadarButtonAreaXStart.Text);
            ini.setValue("Area", "RadarButtonAreaYStart", RadarButtonAreaYStart.Text);
            ini.setValue("Area", "RadarButtonAreaXEnd", RadarButtonAreaXEnd.Text);
            ini.setValue("Area", "RadarButtonAreaYEnd", RadarButtonAreaYEnd.Text);

            // Radar Button area
            ini.setValue("Area", "Radar1ButtonAreaXStart", Radar1ButtonAreaXStart.Text);
            ini.setValue("Area", "Radar1ButtonAreaYStart", Radar1ButtonAreaYStart.Text);
            ini.setValue("Area", "Radar1ButtonAreaXEnd", Radar1ButtonAreaXEnd.Text);
            ini.setValue("Area", "Radar1ButtonAreaYEnd", Radar1ButtonAreaYEnd.Text);
            ini.setValue("Area", "RadarButtonsOffsetY", RadarButtonsOffsetY.Text);

            // Radar Close Button area
            ini.setValue("Area", "RadarCloseButtonAreaXStart", RadarCloseButtonAreaXStart.Text);
            ini.setValue("Area", "RadarCloseButtonAreaYStart", RadarCloseButtonAreaYStart.Text);
            ini.setValue("Area", "RadarCloseButtonAreaXEnd", RadarCloseButtonAreaXEnd.Text);
            ini.setValue("Area", "RadarCloseButtonAreaYEnd", RadarCloseButtonAreaYEnd.Text);

            // Return Maintenance Button area
            ini.setValue("Area", "ReturnMaintenanceButtonAreaXStart", ReturnMaintenanceButtonAreaXStart.Text);
            ini.setValue("Area", "ReturnMaintenanceButtonAreaYStart", ReturnMaintenanceButtonAreaYStart.Text);
            ini.setValue("Area", "ReturnMaintenanceButtonAreaXEnd", ReturnMaintenanceButtonAreaXEnd.Text);
            ini.setValue("Area", "ReturnMaintenanceButtonAreaYEnd", ReturnMaintenanceButtonAreaYEnd.Text);

            // Interval
            // Window Close interval
            ini.setValue("Interval", "WindowOpenInterval", WindowOpenInterval.Text);
            ini.setValue("Interval", "WindowCloseInterval", WindowCloseInterval.Text);
            ini.setValue("Interval", "WindowCloseAnotherInterval", WindowCloseAnotherInterval.Text);

            // Next Check In interval
            ini.setValue("Interval", "CheckInIntervalMin", CheckInIntervalMin.Text);
            ini.setValue("Interval", "CheckInIntervalMax", CheckInIntervalMax.Text);

            // Maintain Link interval
            ini.setValue("Interval", "MaintainLinkIntervalMin", MaintainLinkIntervalMin.Text);
            ini.setValue("Interval", "MaintainLinkIntervalMax", MaintainLinkIntervalMax.Text);

            // Charge Battely interval
            ini.setValue("Interval", "ChargeBattelyInterval", ChargeBattelyInterval.Text);

            // Unlock Interval
            ini.setValue("Interval", "UnlockPowerInterval", UnlockPowerInterval.Text);
            ini.setValue("Interval", "UnlockSwipeInterval", UnlockSwipeInterval.Text);
            ini.setValue("Interval", "UnlockInputInterval", UnlockInputInterval.Text);

            // Wakeup Interval
            ini.setValue("Interval", "WakeupInterval", WakeupInterval.Text);

            // Close Button Interval
            ini.setValue("Interval", "CloseButtonInterval", CloseButtonInterval.Text);

            // Error Button Interval
            ini.setValue("Interval", "ErrorButtonInterval", ErrorButtonInterval.Text);

            // Charge Button Interval
            ini.setValue("Interval", "ChargeButtonInterval", ChargeButtonInterval.Text);

            // Battry Button Interval
            ini.setValue("Interval", "BattryButtonInterval", BattryButtonInterval.Text);

            // Bag Button Interval
            ini.setValue("Interval", "BagButtonInterval", BagButtonInterval.Text);

            // CheckIn flow Interval
            ini.setValue("Interval", "DoCheckInInterval", DoCheckInInterval.Text);
            ini.setValue("Interval", "CloseCheckInInterval", CloseCheckInInterval.Text);

            // ScreenShot Interval
            ini.setValue("Interval", "TakeScreenShotInterval", TakeScreenShotInterval.Text);
            ini.setValue("Interval", "PullFileInterval", PullFileInterval.Text);
            ini.setValue("Interval", "RemoveFileInterval", RemoveFileInterval.Text);

            // Radar Button Interval
            ini.setValue("Interval", "RadarButtonInterval", RadarButtonInterval.Text);

            // Radar Check in Interval
            ini.setValue("Interval", "RadarCheckInInterval", RadarCheckInInterval.Text);

            // Radar Close Button Interval
            ini.setValue("Interval", "RadarCloseButtonInterval", RadarCloseButtonInterval.Text);

            // Radar Check in try
            ini.setValue("Interval", "RadarCheckInTryNum", RadarCheckInTryNum.Text);
            ini.setValue("Interval", "RadarCheckInTryTime", RadarCheckInTryTime.Text);
            ini.setValue("Interval", "RadarCheckInTryIntervalMin", RadarCheckInTryIntervalMin.Text);
            ini.setValue("Interval", "RadarCheckInTryIntervalMax", RadarCheckInTryIntervalMax.Text);

            // Radar Useable num
            ini.setValue("Interval", "RadarUseableNum", RadarUseableNum.Text);

            // Smart Recovery Time
            ini.setValue("Interval", "ToughRecoveryTimeMin", ToughRecoveryTimeMin.Text);
            ini.setValue("Interval", "ToughRecoveryTimeMax", ToughRecoveryTimeMax.Text);

            // Smart Check Alive Interval
            ini.setValue("Interval", "ToughCheckAliveIntervalMin", ToughCheckAliveIntervalMin.Text);
            ini.setValue("Interval", "ToughCheckAliveIntervalMax", ToughCheckAliveIntervalMax.Text);

            // Image
            // ScreenShot Info
            ini.setValue("Image", "ScreenShotTrimX", ScreenShotTrimX.Text);
            ini.setValue("Image", "ScreenShotTrimY", ScreenShotTrimY.Text);
            ini.setValue("Image", "ScreenShotTrimWidth", ScreenShotTrimWidth.Text);
            ini.setValue("Image", "ScreenShotTrimHeight", ScreenShotTrimHeight.Text);

            // ScreenShot Path
            ini.setValue("Image", "ScreenShotRemotePath", ScreenShotRemotePath.Text);
            ini.setValue("Image", "ScreenShotLocalPath", ScreenShotLocalPath.Text);

            // OCR Info
            ini.setValue("Image", "TrimImageLocalPath", TrimImageLocalPath.Text);
            ini.setValue("Image", "OcrTextLocalPath", OcrTextLocalPath.Text);

            // Compared Image Path
            ini.setValue("Image", "ComparedLinkImagePath", ComparedLinkImagePath.Text);
            ini.setValue("Image", "ComparedErrorImagePath", ComparedErrorImagePath.Text);
            ini.setValue("Image", "ComparedGreetingImagePath", ComparedGreetingImagePath.Text);
            ini.setValue("Image", "ComparedLoadingImagePathA", ComparedLoadingImagePathA.Text);
            ini.setValue("Image", "ComparedLoadingImagePathB", ComparedLoadingImagePathB.Text);
            ini.setValue("Image", "ComparedMaintenanceImagePath", ComparedMaintenanceImagePath.Text);
            ini.setValue("Image", "ComparedOpeningImagePath", ComparedOpeningImagePath.Text);
            ini.setValue("Image", "ComparedTitleImagePath", ComparedTitleImagePath.Text);
            ini.setValue("Image", "ComparedTimelineImagePath", ComparedTimelineImagePath.Text);
            
            // Threshold
            // Pattern matching threshold
            ini.setValue("Threshold", "PatternMatchingThreshold", PatternMatchingThreshold.Text);

            // Battely threshold
            ini.setValue("Threshold", "BatteryEnoughThreshold", BatteryEnoughThreshold.Text);
            ini.setValue("Threshold", "BatteryMaxThreshold", BatteryMaxThreshold.Text);

            // No Recover threshold
            ini.setValue("Threshold", "NoRecoverThreshold", NoRecoverThreshold.Text);

            // Unlock
            // Unlock
            ini.setValue("Unlock", "UnlockType", UnlockType.Text);

            ini.setValue("Unlock", "UnlockPassword" , UnlockPassword.Password);
            ini.setValue("Unlock", "UnlockXStart" , UnlockXStart.Text);
            ini.setValue("Unlock", "UnlockYStart" , UnlockYStart.Text);
            ini.setValue("Unlock", "UnlockXEnd" , UnlockXEnd.Text);
            ini.setValue("Unlock", "UnlockYEnd" , UnlockYEnd.Text);
            ini.setValue("Unlock", "UnlockDuration" , UnlockDuration.Text);
        }
        /// <summary>
        /// ConfigのTextが変化したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Config_TextChanged(object sender, TextChangedEventArgs e)
        {
            buttonSave.IsEnabled = true;
        }
        /// <summary>
        /// ConfigのTextが変化したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Config_TextChanged(object sender, RoutedEventArgs e)
        {
            buttonSave.IsEnabled = true;
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            foreach (string stFilePath in System.IO.Directory.GetFiles(@"Preset\", "*.ini"))
            {
                File.Copy(stFilePath, presetPath + System.IO.Path.GetFileName(stFilePath), true);
            }
            ExtractPreset(comboBoxSelectConfig.SelectedItem.ToString());
            buttonSave.IsEnabled = false;
        }
    }
}
