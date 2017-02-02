using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AutoHostsUpdateGUI
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// アプリケーションが終了する時のイベント。
        /// </summary>
        /// <param name="e">イベント データ。</param>
        protected override void OnExit(ExitEventArgs e)
        {
            if (App._mutex == null) { return; }

            // アプリケーション設定の保存

            // ミューテックスの解放
            App._mutex.ReleaseMutex();
            App._mutex.Dispose();
            App._mutex = null;
        }

        /// <summary>
        /// アプリケーションが開始される時のイベント。
        /// </summary>
        /// <param name="e">イベント データ。</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // 多重起動チェック
            App._mutex = new Mutex(false, "AutoHostsUpdateGUI-{004FB87B-D8F3-4595-9AFC-590AAA469430}");
            if (!App._mutex.WaitOne(0, false))
            {
                App._mutex.Close();
                App._mutex = null;
                this.Shutdown();
                return;
            }

            // メイン ウィンドウ表示
            MainWindow window = new MainWindow();
            window.Show();
        }

        /// <summary>
        /// 多重起動を防止する為のミューテックス。
        /// </summary>
        private static Mutex _mutex;
    }
}
