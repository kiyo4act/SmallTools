using UtilitiesLib;

namespace AutoHostsUpdateService
{
    partial class AutoHostsUpdateService
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.eventLog = new System.Diagnostics.EventLog();
            this.timer = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.timer)).BeginInit();
            // 
            // eventLog
            // 
            this.eventLog.EntryWritten += new System.Diagnostics.EntryWrittenEventHandler(this.eventLog_EntryWritten);
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_Elapsed);
            // 
            // AutoHostsUpdateService
            // 
            this.ServiceName = "AutoHostsUpdateService";
            ((System.ComponentModel.ISupportInitialize)(this.eventLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.timer)).EndInit();

            this.eventLog.Source = LocalResources.MGstrEventLogSource;
        }

        #endregion
        private System.Diagnostics.EventLog eventLog;
        private System.Timers.Timer timer;
    }
}
