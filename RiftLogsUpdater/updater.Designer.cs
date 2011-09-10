namespace RiftLogsUpdater
{
    partial class updater
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.updateProgress = new System.Windows.Forms.ProgressBar();
            this.updaterBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // updateProgress
            // 
            this.updateProgress.Location = new System.Drawing.Point(2, 2);
            this.updateProgress.Name = "updateProgress";
            this.updateProgress.Size = new System.Drawing.Size(250, 16);
            this.updateProgress.TabIndex = 0;
            // 
            // updaterBackgroundWorker
            // 
            this.updaterBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.updaterBackgroundWorker_DoWork);
            this.updaterBackgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.updaterBackgroundWorker_ProgressChanged);
            this.updaterBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.updaterBackgroundWorker_RunWorkerCompleted);
            // 
            // updater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(254, 21);
            this.ControlBox = false;
            this.Controls.Add(this.updateProgress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "updater";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Downloading";
            this.Load += new System.EventHandler(this.updater_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar updateProgress;
        private System.ComponentModel.BackgroundWorker updaterBackgroundWorker;
    }
}

