using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyncLocationChangerGUI
{
    public partial class NotifyIconWrapper : Component
    {
        public NotifyIconWrapper()
        {
            InitializeComponent();

            // イベントハンドラの設定
            toolStripMenuItem_Show.Click += toolStripMenuItemShow_Click;
            toolStripMenuItem_Exit.Click += toolStripMenuItemExit_Click;

            ShowWindow();
        }

        // 常駐させるウィンドウはここで保持する
        public MainWindow win = new MainWindow();

        public NotifyIconWrapper(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowWindow();
        }

        void toolStripMenuItemShow_Click(object sender, EventArgs e)
        {
            ShowWindow();
        }

        void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            win.QuitActivity();
        }

        private void ShowWindow()
        {
            // ウィンドウ表示&最前面に持ってくる
            if (win.WindowState == System.Windows.WindowState.Minimized)
                win.WindowState = System.Windows.WindowState.Normal;

            win.Show();
            win.Activate();
            // タスクバーでの表示をする
            win.ShowInTaskbar = true;
        }
    }
}
