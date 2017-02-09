using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NowPlayingForLync
{
    public partial class NotifyIconWrapper : Component
    {
        // 常駐させるウィンドウはここで保持する
        public MainWindow win = new MainWindow();

        public NotifyIconWrapper()
        {
            InitializeComponent();

            this.toolStripMenuItem_Open.Click += ToolStripMenuItem_Open_Click;
            this.toolStripMenuItem_Exit.Click += ToolStripMenuItem_Exit_Click;

            this.toolStripMenuItem_Start.Click += ToolStripMenuItem_Start_Click;
            this.toolStripMenuItem_Stop.Click += ToolStripMenuItem_Stop_Click;

            ShowWindow();
        }

        private void ToolStripMenuItem_Stop_Click(object sender, EventArgs e)
        {
            win.StopMusic();
        }

        private void ToolStripMenuItem_Start_Click(object sender, EventArgs e)
        {
            win.StartMusic();
        }

        private void ToolStripMenuItem_Exit_Click(object sender, EventArgs e)
        {
            win.QuitActivity();
        }

        private void ToolStripMenuItem_Open_Click(object sender, EventArgs e)
        {
            ShowWindow();
        }

        public NotifyIconWrapper(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
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

        private void notifyIcon1_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ShowWindow();
        }
    }
}
