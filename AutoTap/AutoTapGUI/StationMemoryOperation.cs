using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AdbOperations;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;

namespace AutoTapGUI
{
    public class StationMemoryOperation : IAutomateOperation
    {
        private readonly Random _rand;
        private ImageAnalyzer _imageAnalyzer;
        private AndroidDevice _androidDevice;
        private WorkingMode _workingMode;
        private static string ocrLang = "stm";
        private static string ocrConf = "num";
        private WorkingMode _nextMode = WorkingMode.None;
        private int usedRadarNum = 0;
        private DateTime[] radarAttackTimeArray;
        private bool[] isUseBatteryArray;
        private int useRadarOrder = 0;
        public bool fSleepLog = true;
        private bool fLinkConnectionCache = false;
        private DateTime lastCheckLinkConnectionTime = DateTime.Now - TimeSpan.FromSeconds(1);
        public enum WorkingMode
        {
            None,
            Zombie,
            Dominate,
            Tough,
            Defend,
            Recon
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private StationMemoryOperation()
        {
            _rand = new Random();
            _imageAnalyzer = new ImageAnalyzer();
        }
        public StationMemoryOperation(string iniFilePath, WorkingMode workingMode, bool[] battArray, WorkingMode nextMode, int raderOrder, bool sleepLog) : this()
        {
            _workingMode = workingMode;
            _androidDevice = new AndroidDevice(iniFilePath);
            isUseBatteryArray = battArray;
            _nextMode = nextMode;
            useRadarOrder = raderOrder;
            fSleepLog = sleepLog;
        }

        public override void DoAutomate(CancellationToken token)
        {
            Debug.WriteLine("DoAutomate(CancellationToken)");
            if (_workingMode == WorkingMode.Zombie)
            {
                Trace.WriteLine("動作モード: Zombie");
                while (!token.IsCancellationRequested)
                {
                    while (!token.IsCancellationRequested && !IsLinkConnection(token) && IsBatteryEnough())
                    {
                        DoCheckInFlow(token);
                        SleepForNextCheckIn(token);
                    }

                    do
                    {
                        SleepForMaintainLink(token);
                    } while (!token.IsCancellationRequested && IsLinkConnection(token) && IsBatteryEnough());
                    // バッテリーが不十分なら充電
                    if (!token.IsCancellationRequested && !IsBatteryEnough())
                    {
                        ChargeBattery(token);
                    }
                }
            }
            else if (_workingMode == WorkingMode.Dominate)
            {
                Trace.WriteLine("動作モード: Dominate");
                // 次のチェックイン可能時間
                DateTime nextCheckInTime = DateTime.Now;
                while (!token.IsCancellationRequested)
                {
                    // リンクなし・かつバッテリーが十分ならばチェックインループに入る
                    while (!token.IsCancellationRequested && !IsLinkConnection(token) && IsBatteryEnough())
                    {
                        // 前回のチェックインから5分以上経つまで待つ
                        if (DateTime.Now < nextCheckInTime)
                        {
                            SleepForNextCheckIn(nextCheckInTime, token);
                        }

                        // チェックインする
                        DoCheckInFlow(token);

                        // 5分後の時間を次のチェックイン可能時間とする
                        nextCheckInTime = UpdateNextCheckInTime();

                        if (!token.IsCancellationRequested && !IsLinkConnection(token))
                        {
                            // リンクが取れない場合は5分くらいランダムに待つ
                            SleepForNextCheckIn(token);
                        }
                        else
                        {
                            // リンクが取れた場合は保持ループに入るためにこのループを出る
                            break;
                        }
                    }
                    // リンクあり・かつバッテリーが十分ならば保持ループに入る
                    while (!token.IsCancellationRequested && IsLinkConnection(token) && IsBatteryEnough())
                    {
                        Recover(CheckRecoverNum(isUseBatteryArray), token);
                    }
                    // バッテリーが不十分なら充電
                    if (!token.IsCancellationRequested && !IsBatteryEnough())
                    {
                        ChargeBattery(token);
                    }
                }
            }
            else if (_workingMode == WorkingMode.Tough)
            {
                Trace.WriteLine("動作モード: Tough");
                // 次のチェックイン可能時間
                DateTime nextCheckInTime = DateTime.Now;
                DateTime endRecoverLoopTime = DateTime.Now;

                while (!token.IsCancellationRequested)
                {
                    // リンクなし・かつバッテリーが十分ならばチェックインループに入る
                    while (!token.IsCancellationRequested && !IsLinkConnection(token) && IsBatteryEnough())
                    {
                        // 前回のチェックインから5分以上経つまで待つ
                        if (DateTime.Now < nextCheckInTime)
                        {
                            SleepForNextCheckIn(nextCheckInTime, token);
                        }

                        // チェックインする
                        DoCheckInFlow(token);

                        // 5分後の時間を次のチェックイン可能時間とする
                        nextCheckInTime = UpdateNextCheckInTime();

                        if (!token.IsCancellationRequested && !IsLinkConnection(token))
                        {
                            // リンクが取れない場合は5分くらいランダムに待つ
                            SleepForNextCheckIn(token);
                        }
                        else
                        {
                            // リンクが取れた場合は保持ループを終える時間を設定する
                            endRecoverLoopTime = SetEndOfLoopTime();
                            // 保持ループに入るためにこのループを出る
                            break;
                        }
                    }
                    // リンクあり・かつバッテリーが十分・かつ保持ループを終える時間内ならば特定の時間だけ保持ループに入る
                    while (!token.IsCancellationRequested && IsLinkConnection(token) && IsBatteryEnough() && DateTime.Now < endRecoverLoopTime)
                    {
                        Recover(CheckRecoverNum(isUseBatteryArray), token);
                    }
                    // リンクあり・かつバッテリーが十分・かつ保持ループを終える時間を過ぎているならばLinkチェックループに入る
                    while (!token.IsCancellationRequested && IsLinkConnection(token) && IsBatteryEnough() && DateTime.Now >= endRecoverLoopTime)
                    {
                        SleepForNextLinkCheck(token);
                    }

                    // バッテリーが不十分なら充電
                    if (!token.IsCancellationRequested && !IsBatteryEnough())
                    {
                        ChargeBattery(token);
                    }
                }
            }
            else if (_workingMode == WorkingMode.Defend)
            {
                Trace.WriteLine("動作モード: Defend");
                // リンクありならば保持ループに入る
                while (!token.IsCancellationRequested && IsLinkConnection(token))
                {
                    // バッテリーが十分なら回復
                    if (!token.IsCancellationRequested && IsBatteryEnough())
                    {
                        Recover(CheckRecoverNum(isUseBatteryArray), token);
                    }
                    // バッテリーが不十分なら充電
                    else if (!token.IsCancellationRequested && !IsBatteryEnough())
                    {
                        ChargeBattery(token);
                    }
                }
                Trace.WriteLine("---------- Defend 終了 ----------");

                _workingMode = _nextMode;
                DoAutomate(token);
            }
            else if (_workingMode == WorkingMode.Recon)
            {
                Trace.WriteLine("動作モード: Recon");
                // レーダー座標初期化
                InitRadar();
                // 次のチェックイン可能時間
                DateTime nextCheckInTime = DateTime.Now;
                // レーダー攻撃時間配列初期化
                radarAttackTimeArray = new DateTime[_androidDevice.RadarUseableNum];
                // レーダー最大試行回数を超えていなければ続ける
                while (!token.IsCancellationRequested && IsKeepRecon())
                {
                    // リンクなし・かつバッテリーが十分ならばチェックインループに入る
                    while (!token.IsCancellationRequested && !IsLinkConnection(token) && IsBatteryEnough())
                    {
                        // 前回のチェックインから5分以上経つまで待つ
                        if (DateTime.Now < nextCheckInTime)
                        {
                            SleepForNextCheckIn(nextCheckInTime, token);
                        }

                        // レーダーでチェックインする
                        DoRadarCheckInFlow(token);
                        // レーダー攻撃時間回数処理
                        radarAttackTimeArray[usedRadarNum] = DateTime.Now;
                        usedRadarNum++;

                        // 5分後の時間を次のチェックイン可能時間とする
                        nextCheckInTime = UpdateNextCheckInTime();

                        if (!token.IsCancellationRequested && !IsLinkConnection(token))
                        {
                            // レーダー最大試行回数を超えていた場合はループ脱出
                            if (!(!token.IsCancellationRequested && IsKeepRecon())) break;

                            // レーダー試行のクールタイムに入っているか調べる
                            if (!token.IsCancellationRequested && IsRadarAttackable())
                            {
                                // クールタイムを過ごす
                                SleepForRadarCoolTime(token);
                            }
                            else
                            {
                                // リンクが取れない場合は5分くらいランダムに待つ
                                SleepForNextCheckIn(token);
                            }
                        }
                        else
                        {
                            // リンクが取れた場合は保持ループに入るためにこのループを出る
                            break;
                        }
                    }
                    // リンクあり・かつバッテリーが十分ならば保持ループに入る
                    while (!token.IsCancellationRequested && IsLinkConnection(token) && IsBatteryEnough())
                    {
                        Recover(CheckRecoverNum(isUseBatteryArray), token);
                    }

                    if (!token.IsCancellationRequested && !IsBatteryEnough())
                    {
                        ChargeBattery(token);
                    }
                }
                Trace.WriteLine("---------- Recon 終了 ----------");

                _workingMode = _nextMode;
                DoAutomate(token);
            }
            else if (_workingMode == WorkingMode.None)
            {
                Trace.WriteLine("動作モード: None");
                GoToSleepMode();
            }
                return;
        }

        private DateTime SetEndOfLoopTime()
        {
            int offset = _rand.Next(_androidDevice.ToughRecoveryTimeMin, _androidDevice.ToughRecoveryTimeMax);
            return DateTime.Now.AddMilliseconds(offset);
        }

        private void InitRadar()
        {
            Debug.WriteLine("InitRadar()");
            _androidDevice.UseRadarButtonAreaXStart = _androidDevice.Radar1ButtonAreaXStart;
            _androidDevice.UseRadarButtonAreaXEnd = _androidDevice.Radar1ButtonAreaXEnd;

            _androidDevice.UseRadarButtonAreaYStart = _androidDevice.Radar1ButtonAreaYStart + (_androidDevice.RadarButtonsOffsetY * useRadarOrder);
            _androidDevice.UseRadarButtonAreaYEnd = _androidDevice.Radar1ButtonAreaYEnd + (_androidDevice.RadarButtonsOffsetY * useRadarOrder);
        }

        private bool IsRadarAttackable()
        {
            Debug.WriteLine("IsRadarAttackable()");
            bool fResult = false;
            Trace.WriteLine(string.Format("レーダー使用回数: {0}/{1}", usedRadarNum, _androidDevice.RadarCheckInTryNum));
            // RadarCheckInTryNumの回数分攻撃していない場合は許可
            if (usedRadarNum >= _androidDevice.RadarCheckInTryNum)
            {
                Debug.WriteLine(string.Format("usedRadarNum: {0}", usedRadarNum));
                Debug.WriteLine(string.Format("Already attacked {0} times", _androidDevice.RadarCheckInTryNum));
                // usedRadarNum-RadarCheckInTryNumの時間を比較して、RadarCheckInTryTime以内なら不許可
                if ((radarAttackTimeArray[usedRadarNum - 1] - radarAttackTimeArray[usedRadarNum - _androidDevice.RadarCheckInTryNum]) <= new TimeSpan(0, 0, 0, 0, _androidDevice.RadarCheckInTryTime))
                {
                    fResult = true;
                    Debug.WriteLine(string.Format("radarAttackTimeArray[usedRadarNum - 1]: {0}", radarAttackTimeArray[usedRadarNum - 1]));
                    Debug.WriteLine(string.Format("radarAttackTimeArray[usedRadarNum - _androidDevice.RadarCheckInTryNum]: {0}", radarAttackTimeArray[usedRadarNum - _androidDevice.RadarCheckInTryNum]));
                    Debug.WriteLine(string.Format("Radar Attack Time Span: {0}", (radarAttackTimeArray[usedRadarNum - 1] - radarAttackTimeArray[usedRadarNum - _androidDevice.RadarCheckInTryNum])));
                }
                else
                {

                }
            }
            else
            {
                Debug.WriteLine(string.Format("usedRadarNum: {0}", usedRadarNum));
                Debug.WriteLine(string.Format("Still not attacked {0} times", _androidDevice.RadarCheckInTryNum));
            }

            Trace.WriteLine(string.Format("レーダー攻撃可能？･･･ {0}", fResult));
            return fResult;
        }

        private void DoRadarCheckInFlow(CancellationToken token)
        {
            Debug.WriteLine("DoRadarCheckInFlow(CancellationToken)");
            TapBagButton();
            Sleep("BagButtonInterval", _androidDevice.BagButtonInterval, token);
            TapRadarButton();
            Sleep("RadarButtonInterval", _androidDevice.RadarButtonInterval, token);
            TapUseRadarButton();
            Sleep("RadarCheckInInterval", _androidDevice.RadarCheckInInterval, token);
            Trace.WriteLine("---------- レーダーで攻撃！ ----------");
            TapRadarCloseClose();
            Sleep("RadarCloseButtonInterval", _androidDevice.RadarCloseButtonInterval, token);
        }

        private bool IsKeepRecon()
        {
            Debug.WriteLine("IsKeepRecon()");
            bool fResult = (_androidDevice.RadarUseableNum > usedRadarNum);
            Debug.WriteLine(string.Format("RadarUseableNum:{0} | usedRadarNum:{1}", _androidDevice.RadarUseableNum, usedRadarNum));
            if (!fResult) Debug.WriteLine("----------Radar End----------");
            return fResult;
        }

        private DateTime UpdateNextCheckInTime()
        {
            Debug.WriteLine("UpdateNextCheckInTime()");
            TimeSpan sleepInterval = new TimeSpan(0, 5, 0);
            return DateTime.Now.Add(sleepInterval);
        }
        private bool IsLinkConnection(CancellationToken token)
        {
            return IsLinkConnection(false, token);
        }
        private bool IsLinkConnection(bool fUseCache, CancellationToken token)
        {
            Debug.WriteLine("IsLinkConnection(bool, CancellationToken)");
            bool fResult;
            bool loop = false;
            do
            {
                if (lastCheckLinkConnectionTime + TimeSpan.FromSeconds(1) < DateTime.Now || !fUseCache)
                {
                    // Push Link Button
                    DoLinkButtonFlow(token);

                    // Take Screenshot
                    TakeScreenShot(token);

                    // Image Analyze
                    fResult = CheckLinkImage();
                    if (!fResult && !CheckTimelineImage())
                    {
                        loop = EscapeFromOtherSituations(token);
                    }
                    fLinkConnectionCache = fResult;
                    lastCheckLinkConnectionTime = DateTime.Now;
                }
                else
                {
                    Trace.WriteLine("Link継続中かを1秒以内に判断した結果を再利用します");
                    fResult = fLinkConnectionCache;
                }
            } while (loop);

            return fResult;
        }

        private bool EscapeFromOtherSituations(CancellationToken token)
        {
            if (CheckGreetingImage())
            {
                Debug.WriteLine("Greeting image is founded!");
                // メンテナンスから戻るボタンを押す
                TapMaintenanceEscapeButton(token);
                // 30秒待つ
                Sleep("EscapeGreetingImage", 30000, token);
                return true;
            }
            else if (CheckLoadingImageA())
            {
                Debug.WriteLine("Loading image A is founded!");
                // 30秒待つ
                Sleep("EscapeLoadingImageA", 30000, token);
                return true;
            }
            else if (CheckLoadingImageB())
            {
                Debug.WriteLine("Loading image B is founded!");
                // 30秒待つ
                Sleep("EscapeLoadingImageB", 30000, token);
                return true;
            }
            else if (CheckMaintenanceImage())
            {
                Debug.WriteLine("Maintenance image is founded!");
                // メンテナンスから戻るボタンを押す
                TapMaintenanceEscapeButton(token);
                // 30秒待つ
                Sleep("EscapeLoadingImage", 30000, token);
                return true;
            }
            else if (CheckOpeningImage())
            {
                Debug.WriteLine("Opening image is founded!");
                // 10秒待つ
                Sleep("EscapeLoadingImage", 10000, token);
                return true;
            }
            else if (CheckTitleImage())
            {
                Debug.WriteLine("Title image is founded!");
                // メンテナンスから戻るボタンを押す
                TapMaintenanceEscapeButton(token);
                // 30秒待つ
                Sleep("EscapeLoadingImage", 30000, token);
                return true;
            }
            /* Errorチェック
            else if (CheckErrorImage())
            {
                Debug.WriteLine("Error image is founded!");
                // エラーから戻るボタンを押す
                TapErrorEscapeButton(token);
                // 10秒待つ
                Sleep("EscapeGreetingImage", 10000, token);
                return true;
            }*/
            else
            {
                Debug.WriteLine("Other image is founded!");
                Debug.WriteLine("Throw Exception");
                Trace.WriteLine("### 駅メモではないと判断して例外をスローしました ###");
                throw new System.NotSupportedException("Current Foreground Application is not Station Memory.");
            }
        }

        private bool CheckErrorImage()
        {
            Debug.WriteLine("CheckErrorImage()");
            _imageAnalyzer = new ImageAnalyzer(_androidDevice.ScreenShotLocalPath, _androidDevice.ComparedErrorImagePath);
            bool fResult = _imageAnalyzer.IsPatternMatching(_androidDevice.PatternMatchingThreshold);
            Trace.WriteLine(string.Format("エラー画像だった？･･･ {0}", fResult));

            return fResult;
        }
        private bool CheckGreetingImage()
        {
            Debug.WriteLine("CheckGreetingImage()");
            _imageAnalyzer = new ImageAnalyzer(_androidDevice.ScreenShotLocalPath, _androidDevice.ComparedGreetingImagePath);
            bool fResult = _imageAnalyzer.IsPatternMatching(_androidDevice.PatternMatchingThreshold);
            Trace.WriteLine(string.Format("あいさつ画像だった？･･･ {0}", fResult));

            return fResult;
        }
        private bool CheckLoadingImageA()
        {
            Debug.WriteLine("CheckLoadingImage()");
            _imageAnalyzer = new ImageAnalyzer(_androidDevice.ScreenShotLocalPath, _androidDevice.ComparedLoadingImagePathA);
            bool fResult = _imageAnalyzer.IsPatternMatching(_androidDevice.PatternMatchingThreshold);
            Trace.WriteLine(string.Format("ロード中画像Aだった？･･･ {0}", fResult));

            return fResult;
        }
        private bool CheckLoadingImageB()
        {
            Debug.WriteLine("CheckLoadingImage()");
            _imageAnalyzer = new ImageAnalyzer(_androidDevice.ScreenShotLocalPath, _androidDevice.ComparedLoadingImagePathB);
            bool fResult = _imageAnalyzer.IsPatternMatching(_androidDevice.PatternMatchingThreshold);
            Trace.WriteLine(string.Format("ロード中画像Bだった？･･･ {0}", fResult));

            return fResult;
        }
        private bool CheckMaintenanceImage()
        {
            Debug.WriteLine("CheckMaintenanceImage()");
            _imageAnalyzer = new ImageAnalyzer(_androidDevice.ScreenShotLocalPath, _androidDevice.ComparedMaintenanceImagePath);
            bool fResult = _imageAnalyzer.IsPatternMatching(_androidDevice.PatternMatchingThreshold);
            Trace.WriteLine(string.Format("メンテナンス画像だった？･･･ {0}", fResult));

            return fResult;
        }
        private bool CheckOpeningImage()
        {
            Debug.WriteLine("CheckOpeningImage()");
            _imageAnalyzer = new ImageAnalyzer(_androidDevice.ScreenShotLocalPath, _androidDevice.ComparedOpeningImagePath);
            bool fResult = _imageAnalyzer.IsPatternMatching(_androidDevice.PatternMatchingThreshold);
            Trace.WriteLine(string.Format("起動画像だった？･･･ {0}", fResult));

            return fResult;
        }
        private bool CheckTitleImage()
        {
            Debug.WriteLine("CheckTitleImage()");
            _imageAnalyzer = new ImageAnalyzer(_androidDevice.ScreenShotLocalPath, _androidDevice.ComparedTitleImagePath);
            bool fResult = _imageAnalyzer.IsPatternMatching(_androidDevice.PatternMatchingThreshold);
            Trace.WriteLine(string.Format("タイトル画像だった？･･･ {0}", fResult));

            return fResult;
        }

        private bool CheckLinkImage()
        {
            Debug.WriteLine("CheckLinkImage()");
            _imageAnalyzer = new ImageAnalyzer(_androidDevice.ScreenShotLocalPath, _androidDevice.ComparedLinkImagePath);
            bool fResult = _imageAnalyzer.IsPatternMatching(_androidDevice.PatternMatchingThreshold);
            Trace.WriteLine(string.Format("リンク継続中？･･･ {0}", fResult));

            return fResult;
        }
        private bool CheckTimelineImage()
        {
            Debug.WriteLine("CheckTimelineImage()");
            _imageAnalyzer = new ImageAnalyzer(_androidDevice.ScreenShotLocalPath, _androidDevice.ComparedTimelineImagePath);
            bool fResult = _imageAnalyzer.IsPatternMatching(_androidDevice.PatternMatchingThreshold);
            Trace.WriteLine(string.Format("タイムラインだった？･･･ {0}", fResult));

            return fResult;
        }

        private void Recover(int[] recoverNumArray, CancellationToken token)
        {
            Debug.WriteLine("Recover(int[], CancellationToken)");
            if (recoverNumArray[0] > 0 || recoverNumArray[1] > 0 || recoverNumArray[2] > 0)
            {
                Trace.WriteLine(string.Format("---------- でんこ充電開始 50:{0}個, 150:{1}個, 300:{2}個 ----------", recoverNumArray[0], recoverNumArray[1], recoverNumArray[2]));
                TapBagButton(token);
                TapBatteryButton(token);

                for (int i = 0; i < recoverNumArray[2]; i++)
                {
                    TapCharge300Button(token);
                }

                for (int i = 0; i < recoverNumArray[1]; i++)
                {
                    TapCharge150Button(token);
                }

                for (int i = 0; i < recoverNumArray[0]; i++)
                {
                    TapCharge50Button(token);
                }

                TapCloseButton(token);

                // 例外の場合に備えてWindowCloseを3回行う
                TapWindowCloseAnother(token);
                TapWindowCloseAnother(token);
                TapWindowCloseAnother(token);
                Trace.WriteLine("---------- でんこ充電完了 ----------");
            }
            else
            {
                Trace.WriteLine("---------- でんこ充電の必要なし ----------");
            }

        }

        private int[] CheckRecoverNum(bool[] recoverCheckArray)
        {
            int[] resultArray = { 0, 0, 0 };
            if (!recoverCheckArray[0] && !recoverCheckArray[1] && !recoverCheckArray[2]) return resultArray;

            Debug.WriteLine("CheckRecoverNum(bool[])");
            string hp;
            int currentHp = -1;
            int maxHp = -1;

            // トリミング
            _imageAnalyzer.TrimInstanceImage(_androidDevice.ScreenShotTrimX, _androidDevice.ScreenShotTrimY, _androidDevice.ScreenShotTrimWidth, _androidDevice.ScreenShotTrimHeight);

            // 画像出力
            _imageAnalyzer.SaveImage(_androidDevice.TrimImageLocalPath);

            // OCRして出力テキストをファイルに保存
            OcrOperation.DoOcr(_androidDevice.TrimImageLocalPath, _androidDevice.OcrTextLocalPath, ocrLang, ocrConf);

            // 出力テキストを読み込む
            using(StreamReader sr = new StreamReader(_androidDevice.OcrTextLocalPath+".txt"))
            {
                hp = sr.ReadLine();
                Trace.WriteLine(string.Format("でんこの体力: {0}", hp));
                if (hp[0] == '0')
                {
                    Trace.WriteLine("例外的な体力の値を検知しました");
                    return resultArray;
                }
            }

            // "/" で区切る
            try
            {
                currentHp = int.Parse(hp.Split('/')[0]);
                maxHp = int.Parse(hp.Split('/')[1]);
            }
            catch
            {
                Trace.WriteLine("エラー: 体力値をパース中に例外が発生しました");
                return resultArray;
            }

            // 例外処理
            if (currentHp > maxHp || currentHp == -1 || maxHp == -1)
            {
                Trace.WriteLine("エラー: 体力値をパースした結果に例外的な値が含まれています");
                return resultArray;
            }

            // 必要な回復回数を計算
            if (recoverCheckArray[2])
            {
                while (currentHp + 300 <= maxHp)
                {
                    resultArray[2]++;
                    currentHp += 300;
                }
            }
            if (recoverCheckArray[1])
            {
                while (currentHp + 150 <= maxHp)
                {
                    resultArray[1]++;
                    currentHp += 150;
                }
            }
            if (recoverCheckArray[0])
            {
                currentHp += _androidDevice.NoRecoverThreshold;
                while (currentHp < maxHp)
                {
                    resultArray[0]++;
                    currentHp += 50;
                }
            }
            
            return resultArray;
        }

        private int GetBattery()
        {
            Debug.WriteLine("GetBattery()");
            return AdbOperation.GetBatteryInfo();
        }
        private bool IsBatteryEnough()
        {
            Debug.WriteLine("IsBatteryEnough()");
            int battery = GetBattery();
            Trace.WriteLine(string.Format("端末バッテリー残量: {0}%", battery));
            bool fResult = _androidDevice.BatteryEnoughThreshold < battery;

            return fResult;
        }

        private bool IsBatteryMax()
        {
            Debug.WriteLine("IsBatteryMax()");
            int battery = GetBattery();
            Trace.WriteLine(string.Format("端末バッテリー残量: {0}%", battery));
            bool fResult = _androidDevice.BatteryMaxThreshold <= battery;

            return fResult;
        }

        private void ChargeBattery(CancellationToken token)
        {
            Debug.WriteLine("ChargeBattery(CancellationToken)");
            Trace.WriteLine("---------- 端末の充電を開始 ----------");
            GoToSleepMode();

            while (!token.IsCancellationRequested && !IsBatteryMax())
            {
                Sleep("ChargeBattelyInterval", _androidDevice.ChargeBattelyInterval, token);
            }
            Trace.WriteLine("---------- 端末の充電が完了 ----------");
        }
        private void TakeScreenShot(CancellationToken token)
        {
            Debug.WriteLine("TakeScreenShot(CancellationToken)");
            Trace.WriteLine("---------- スクリーンショット処理の開始 ----------");
            Trace.WriteLine(string.Format("スクリーンショットを撮影: {0}", _androidDevice.ScreenShotRemotePath));
            AdbOperation.TakeScreenshot(_androidDevice.ScreenShotRemotePath);
            Sleep("TakeScreenShotInterval", _androidDevice.TakeScreenShotInterval, token);
            Trace.WriteLine(string.Format("スクリーンショットを転送: {0}", _androidDevice.ScreenShotLocalPath));
            AdbOperation.PullFile(_androidDevice.ScreenShotRemotePath, _androidDevice.ScreenShotLocalPath);
            Sleep("PullFileInterval", _androidDevice.PullFileInterval, token);
            Trace.WriteLine(string.Format("スクリーンショットを削除: {0}", _androidDevice.ScreenShotRemotePath));
            AdbOperation.RemoveFile(_androidDevice.ScreenShotRemotePath);
            Sleep("RemoveFileInterval", _androidDevice.RemoveFileInterval, token);
            Trace.WriteLine("---------- スクリーンショット処理の終了 ----------");
        }
        private void DoCheckInFlow(CancellationToken token)
        {
            Debug.WriteLine("DoCheckInFlow(CancellationToken)");
            TapCheckInButton();
            Sleep("DoCheckInInterval", _androidDevice.DoCheckInInterval, token);
            TapWindowClose();
            Sleep("CloseCheckInInterval", _androidDevice.CloseCheckInInterval, token);
        }
        private void DoLinkButtonFlow(CancellationToken token)
        {
            Debug.WriteLine("DoLinkButtonFlow(CancellationToken)");
            TapLinkButton();
            Sleep("WindowOpenInterval", _androidDevice.WindowOpenInterval, token);
            TapWindowClose();
            Sleep("WindowCloseInterval", _androidDevice.WindowCloseInterval, token);
        }
        private void TapCheckInButton()
        {
            Debug.WriteLine("TapCheckInButton()");
            int xPosition = _rand.Next(_androidDevice.CheckInAreaXStart, _androidDevice.CheckInAreaXEnd);
            int yPosition = _rand.Next(_androidDevice.CheckInAreaYStart, _androidDevice.CheckInAreaYEnd);
            Trace.WriteLine(string.Format(string.Format("「Check in」ボタンをタップ: X={0}, Y={1}", xPosition, yPosition)));
            AdbOperation.Tap(xPosition, yPosition);
        }
        private void TapLinkButton()
        {
            Debug.WriteLine("TapLinkButton()");
            int xPosition = _rand.Next(_androidDevice.LinkAreaXStart, _androidDevice.LinkAreaXEnd);
            int yPosition = _rand.Next(_androidDevice.LinkAreaYStart, _androidDevice.LinkAreaYEnd);
            Trace.WriteLine(string.Format("「link」ボタンをタップ: X={0}, Y={1}", xPosition, yPosition));
            AdbOperation.Tap(xPosition, yPosition);
        }
        private void TapBagButton()
        {
            Debug.WriteLine("TapBagButton()");
            int xPosition = _rand.Next(_androidDevice.BagButtonAreaXStart, _androidDevice.BagButtonAreaXEnd);
            int yPosition = _rand.Next(_androidDevice.BagButtonAreaYStart, _androidDevice.BagButtonAreaYEnd);
            Trace.WriteLine(string.Format("「かばん」ボタンをタップ: X={0}, Y={1}", xPosition, yPosition));
            AdbOperation.Tap(xPosition, yPosition);
        }
        private void TapBagButton(CancellationToken token)
        {
            Debug.WriteLine("TapBagButton(CancellationToken)");
            TapBagButton();
            Sleep("BagButtonInterval", _androidDevice.BagButtonInterval, token);
        }
        private void TapBatteryButton()
        {
            Debug.WriteLine("TapBatteryButton()");
            int xPosition = _rand.Next(_androidDevice.BatteryButtonAreaXStart, _androidDevice.BatteryButtonAreaXEnd);
            int yPosition = _rand.Next(_androidDevice.BatteryButtonAreaYStart, _androidDevice.BatteryButtonAreaYEnd);
            Trace.WriteLine(string.Format("「バッテリー」ボタンをタップ: X={0}, Y={1}", xPosition, yPosition));
            AdbOperation.Tap(xPosition, yPosition);
        }
        private void TapBatteryButton(CancellationToken token)
        {
            Debug.WriteLine("TapBatteryButton(CancellationToken)");
            TapBatteryButton();
            Sleep("BattryButtonInterval", _androidDevice.BattryButtonInterval, token);
        }
        private void TapCharge50Button()
        {
            Debug.WriteLine(nameof(TapCharge50Button));
            int xPosition = _rand.Next(_androidDevice.Charge50ButtonAreaXStart, _androidDevice.Charge50ButtonAreaXEnd);
            int yPosition = _rand.Next(_androidDevice.Charge50ButtonAreaYStart, _androidDevice.Charge50ButtonAreaYEnd);
            Trace.WriteLine(string.Format("「バッテリー50」ボタンをタップ: X={0}, Y={1}", xPosition, yPosition));
            AdbOperation.Tap(xPosition, yPosition);
        }
        private void TapCharge150Button()
        {
            Debug.WriteLine(nameof(TapCharge150Button));
            int xPosition = _rand.Next(_androidDevice.Charge150ButtonAreaXStart, _androidDevice.Charge150ButtonAreaXEnd);
            int yPosition = _rand.Next(_androidDevice.Charge150ButtonAreaYStart, _androidDevice.Charge150ButtonAreaYEnd);
            Trace.WriteLine(string.Format("「バッテリー150」ボタンをタップ: X={0}, Y={1}", xPosition, yPosition));
            AdbOperation.Tap(xPosition, yPosition);
        }
        private void TapCharge300Button()
        {
            Debug.WriteLine(nameof(TapCharge300Button));
            int xPosition = _rand.Next(_androidDevice.Charge300ButtonAreaXStart, _androidDevice.Charge300ButtonAreaXEnd);
            int yPosition = _rand.Next(_androidDevice.Charge300ButtonAreaYStart, _androidDevice.Charge300ButtonAreaYEnd);
            Trace.WriteLine(string.Format("「バッテリー300」ボタンをタップ: X={0}, Y={1}", xPosition, yPosition));
            AdbOperation.Tap(xPosition, yPosition);
        }
        private void TapCharge50Button(CancellationToken token)
        {
            Debug.WriteLine("TapCharge50Button(CancellationToken)");
            TapCharge50Button();
            Sleep("ChargeButtonInterval", _androidDevice.ChargeButtonInterval, token);
        }
        private void TapCharge150Button(CancellationToken token)
        {
            Debug.WriteLine("TapCharge150Button(CancellationToken)");
            TapCharge150Button();
            Sleep("ChargeButtonInterval", _androidDevice.ChargeButtonInterval, token);
        }
        private void TapCharge300Button(CancellationToken token)
        {
            Debug.WriteLine("TapCharge300Button(CancellationToken)");
            TapCharge300Button();
            Sleep("ChargeButtonInterval", _androidDevice.ChargeButtonInterval, token);
        }
        private void TapCloseButton()
        {
            Debug.WriteLine("TapCloseButton()");
            int xPosition = _rand.Next(_androidDevice.CloseButtonAreaXStart, _androidDevice.CloseButtonAreaXEnd);
            int yPosition = _rand.Next(_androidDevice.CloseButtonAreaYStart, _androidDevice.CloseButtonAreaYEnd);
            Trace.WriteLine(string.Format("「Close」ボタンをタップ: X={0}, Y={1}", xPosition, yPosition));
            AdbOperation.Tap(xPosition, yPosition);
        }
        private void TapCloseButton(CancellationToken token)
        {
            Debug.WriteLine("TapCloseButton(CancellationToken)");
            TapCloseButton();
            Sleep("CloseButtonInterval", _androidDevice.CloseButtonInterval, token);
        }
        private void TapErrorEscapeButton()
        {
            Debug.WriteLine("TapErrorEscapeButton()");
            int xPosition = _rand.Next(_androidDevice.ErrorButtonAreaXStart, _androidDevice.ErrorButtonAreaXEnd);
            int yPosition = _rand.Next(_androidDevice.ErrorButtonAreaYStart, _androidDevice.ErrorButtonAreaYEnd);
            Trace.WriteLine(string.Format("エラーから抜けるボタンをタップ: X={0}, Y={1}", xPosition, yPosition));
            AdbOperation.Tap(xPosition, yPosition);
        }
        private void TapErrorEscapeButton(CancellationToken token)
        {
            Debug.WriteLine("TapErrorEscapeButton(CancellationToken)");
            TapErrorEscapeButton();
            Sleep("TapErrorEscapeButton", _androidDevice.ErrorButtonInterval, token);
        }
        private void TapMaintenanceEscapeButton()
        {
            Debug.WriteLine("TapErrorEscapeButton()");
            int xPosition = _rand.Next(_androidDevice.ReturnMaintenanceButtonAreaXStart, _androidDevice.ReturnMaintenanceButtonAreaXEnd);
            int yPosition = _rand.Next(_androidDevice.ReturnMaintenanceButtonAreaYStart, _androidDevice.ReturnMaintenanceButtonAreaYEnd);
            Trace.WriteLine(string.Format("メンテナンスから抜けるボタンをタップ: X={0}, Y={1}", xPosition, yPosition));
            AdbOperation.Tap(xPosition, yPosition);
        }
        private void TapMaintenanceEscapeButton(CancellationToken token)
        {
            Debug.WriteLine("TapMaintenanceEscapeButton(CancellationToken)");
            TapMaintenanceEscapeButton();
            Sleep("TapMaintenanceEscapeButton", _androidDevice.ErrorButtonInterval, token);
        }
        private void TapWindowClose()
        {
            Debug.WriteLine("TapWindowClose()");
            int xPosition = _rand.Next(_androidDevice.WindowCloseAreaXStart, _androidDevice.WindowCloseAreaXEnd);
            int yPosition = _rand.Next(_androidDevice.WindowCloseAreaYStart, _androidDevice.WindowCloseAreaYEnd);
            Trace.WriteLine(string.Format("linkを閉じるエリアをタップ: X={0}, Y={1}", xPosition, yPosition));
            AdbOperation.Tap(xPosition, yPosition);
        }
        private void TapWindowCloseAnother(CancellationToken token)
        {
            Debug.WriteLine("TapWindowCloseAnother(CancellationToken)");
            TapWindowClose();
            Sleep("WindowCloseAnotherInterval", _androidDevice.WindowCloseAnotherInterval, token);
        }
        private void TapRadarButton()
        {
            Debug.WriteLine("TapRadarButton()");
            int xPosition = _rand.Next(_androidDevice.RadarButtonAreaXStart, _androidDevice.RadarButtonAreaXEnd);
            int yPosition = _rand.Next(_androidDevice.RadarButtonAreaYStart, _androidDevice.RadarButtonAreaYEnd);
            Trace.WriteLine(string.Format("「レーダー」ボタンをタップ: X={0}, Y={1}", xPosition, yPosition));
            AdbOperation.Tap(xPosition, yPosition);
        }
        private void TapUseRadarButton()
        {
            Debug.WriteLine("TapUseRadarButton()");
            int xPosition = _rand.Next(_androidDevice.UseRadarButtonAreaXStart, _androidDevice.UseRadarButtonAreaXEnd);
            int yPosition = _rand.Next(_androidDevice.UseRadarButtonAreaYStart, _androidDevice.UseRadarButtonAreaYEnd);
            Trace.WriteLine(string.Format("「アクセス（レーダー）」ボタンをタップ: X={0}, Y={1}", xPosition, yPosition));
            AdbOperation.Tap(xPosition, yPosition);
        }
        private void TapRadarCloseClose()
        {
            Debug.WriteLine("TapRadarCloseClose()");
            int xPosition = _rand.Next(_androidDevice.RadarCloseButtonAreaXStart, _androidDevice.RadarCloseButtonAreaXEnd);
            int yPosition = _rand.Next(_androidDevice.RadarCloseButtonAreaYStart, _androidDevice.RadarCloseButtonAreaYEnd);
            Trace.WriteLine(string.Format("「Close（レーダー）」ボタンをタップ: X={0}, Y={1}", xPosition, yPosition));
            AdbOperation.Tap(xPosition, yPosition);
        }
        private void GoToSleepMode()
        {
            Debug.WriteLine("GoToSleepMode()");
            Trace.WriteLine("スリープモードへ移行");
            AdbOperation.PushPowerButton();
        }
        private void WakeUp(CancellationToken token)
        {
            Debug.WriteLine("WakeUp(CancellationToken)");
            Unlock(token);
            Sleep("WakeupInterval", _androidDevice.WakeupInterval, token);
        }
        private void Unlock(CancellationToken token)
        {
            Debug.WriteLine("Unlock(CancellationToken)");
            Trace.WriteLine("---------- スリープモードから復帰開始 ----------");
            AdbOperation.PushPowerButton();
            Sleep("UnlockPowerInterval", _androidDevice.UnlockPowerInterval, token);
            switch (_androidDevice.UnlockType)
            {
                case AndroidDevice.UnlockTypeEnum.None:
                    Trace.WriteLine("端末アンロック: スワイプ");
                    AdbOperation.Swipe(_androidDevice.UnlockXStart, _androidDevice.UnlockYStart, _androidDevice.UnlockXEnd, _androidDevice.UnlockYEnd, _androidDevice.UnlockDuration);
                    break;
                case AndroidDevice.UnlockTypeEnum.Pin:
                    Trace.WriteLine("端末アンロック: スワイプ");
                    AdbOperation.Swipe(_androidDevice.UnlockXStart, _androidDevice.UnlockYStart, _androidDevice.UnlockXEnd, _androidDevice.UnlockYEnd, _androidDevice.UnlockDuration);
                    Sleep("UnlockSwipeInterval", _androidDevice.UnlockSwipeInterval, token);
                    Trace.WriteLine("端末アンロック: PIN入力");
                    AdbOperation.InputText(_androidDevice.UnlockPassword);
                    Sleep("UnlockInputInterval", _androidDevice.UnlockInputInterval, token);
                    Trace.WriteLine("端末アンロック: Enter押下");
                    AdbOperation.InputKeyEvent("KEYCODE_ENTER");
                    break;
                case AndroidDevice.UnlockTypeEnum.PinWithoutEnter:
                    Trace.WriteLine("端末アンロック: スワイプ");
                    AdbOperation.Swipe(_androidDevice.UnlockXStart, _androidDevice.UnlockYStart, _androidDevice.UnlockXEnd, _androidDevice.UnlockYEnd, _androidDevice.UnlockDuration);
                    Sleep("UnlockSwipeInterval", _androidDevice.UnlockSwipeInterval, token);
                    Trace.WriteLine("端末アンロック: PIN入力");
                    AdbOperation.InputText(_androidDevice.UnlockPassword);
                    Sleep("UnlockInputInterval", _androidDevice.UnlockInputInterval, token);
                    break;
                case AndroidDevice.UnlockTypeEnum.Passowrd:
                    Trace.WriteLine("端末アンロック: スワイプ");
                    AdbOperation.Swipe(_androidDevice.UnlockXStart, _androidDevice.UnlockYStart, _androidDevice.UnlockXEnd, _androidDevice.UnlockYEnd, _androidDevice.UnlockDuration);
                    Sleep("UnlockSwipeInterval", _androidDevice.UnlockSwipeInterval, token);
                    Trace.WriteLine("端末アンロック: Password入力");
                    AdbOperation.InputText(_androidDevice.UnlockPassword);
                    Sleep("UnlockInputInterval", _androidDevice.UnlockInputInterval, token);
                    Trace.WriteLine("端末アンロック: Enter押下");
                    AdbOperation.InputKeyEvent("KEYCODE_ENTER");
                    break;
            }
            Trace.WriteLine("---------- スリープモードから復帰完了 ----------");
        }

        private void Sleep(string sleepName, int sleepTime, CancellationToken token)
        {
            if (fSleepLog) Trace.WriteLine(string.Format("スリープ - {0}: {1}ms", sleepName, sleepTime));
            try
            {
                Utilities.CancellationTokenEx.CancellableSleep(token, sleepTime);
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("スリープが中断されました");
                return;
            }
        }
        private void SleepForNextCheckIn(CancellationToken token)
        {
            Debug.WriteLine("SleepForNextCheckIn(CancellationToken)");
            GoToSleepMode();
            int sleepTime = _rand.Next(_androidDevice.CheckInIntervalMin, _androidDevice.CheckInIntervalMax);
            DateTime dateTime = DateTime.Now.AddMilliseconds(sleepTime);
            Trace.WriteLine(string.Format("次のチェックイン時間までスリープ: {0} まで", dateTime.ToString("T")));
            try
            {
                Utilities.CancellationTokenEx.CancellableSleep(token, sleepTime);
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("スリープが中断されました");
                return;
            }
            WakeUp(token);
        }

        // レーダーのクールタイム
        private void SleepForRadarCoolTime(CancellationToken token)
        {
            SleepForCoolTime(_androidDevice.RadarCheckInTryIntervalMin, _androidDevice.RadarCheckInTryIntervalMax, token);
        }
        // 保持確認のクールタイム
        private void SleepForNextLinkCheck(CancellationToken token)
        {
            SleepForCoolTime(_androidDevice.ToughCheckAliveIntervalMin, _androidDevice.ToughCheckAliveIntervalMax, token);
        }
        private void SleepForCoolTime(int sleepTimeMin, int sleepTimeMax, CancellationToken token)
        {
            SleepForCoolTime(_rand.Next(sleepTimeMin, sleepTimeMax), token);
        }
        private void SleepForCoolTime(int sleepTime, CancellationToken token)
        {
            Debug.WriteLine("SleepForCoolTime(int, CancellationToken)");
            GoToSleepMode();
            DateTime dateTime = DateTime.Now.AddMilliseconds(sleepTime);
            Trace.WriteLine(string.Format("次のアクションまでのクールタイム: {0} まで", dateTime.ToString("T")));
            try
            {
                Utilities.CancellationTokenEx.CancellableSleep(token, sleepTime);
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("スリープが中断されました");
                return;
            }
            WakeUp(token);
        }

        // 保持中に切られた場合の待ち関数
        private void SleepForNextCheckIn(DateTime nextCheckInTime, CancellationToken token)
        {
            Debug.WriteLine("SleepForNextCheckIn(DateTime, CancellationToken)");
            GoToSleepMode();
            // 元々のインターバルからランダムの広がりを求める
            int sleepTimeMaxMin = _androidDevice.CheckInIntervalMax - _androidDevice.CheckInIntervalMin;
            // 次のチェックインと現在時刻の差をTimeSpanで表す
            TimeSpan minTimeSpan = nextCheckInTime - DateTime.Now;
            // 上記数値から最速～最速+差分のランダム数値を取得
            int sleepTime = _rand.Next((int)minTimeSpan.TotalMilliseconds, (int)minTimeSpan.TotalMilliseconds + sleepTimeMaxMin);
            DateTime dateTime = DateTime.Now.AddMilliseconds(sleepTime);
            Trace.WriteLine(string.Format("次のチェックイン時間までスリープ: {0} まで", dateTime.ToString("T")));
            try
            {
                Utilities.CancellationTokenEx.CancellableSleep(token, sleepTime);
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("スリープが中断されました");
                return;
            }
            
            WakeUp(token);
        }

        private void SleepForMaintainLink(CancellationToken token)
        {
            int sleepTime = _rand.Next(_androidDevice.MaintainLinkIntervalMin, _androidDevice.MaintainLinkIntervalMax);
            DateTime dateTime = DateTime.Now.AddMilliseconds(sleepTime);
            Trace.WriteLine(string.Format("次のLink確認までのスリープ: {0} まで", dateTime.ToString("T")));
            try
            {
                Utilities.CancellationTokenEx.CancellableSleep(token, sleepTime);
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("スリープが中断されました");
                return;
            }
        }
    }

}
