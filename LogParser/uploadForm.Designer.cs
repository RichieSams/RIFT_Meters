namespace LogParser
{
    partial class riftLogsUploader
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
            this.txt_fileDir = new System.Windows.Forms.TextBox();
            this.fileBrowseButton = new System.Windows.Forms.Button();
            this.uploadButton = new System.Windows.Forms.Button();
            this.uploadProgress = new System.Windows.Forms.ProgressBar();
            this.uploadBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.lbl_statusTxt = new System.Windows.Forms.Label();
            this.txt_userName = new System.Windows.Forms.TextBox();
            this.txt_pass = new System.Windows.Forms.TextBox();
            this.lbl_pass = new System.Windows.Forms.Label();
            this.lbl_userName = new System.Windows.Forms.Label();
            this.loginButton = new System.Windows.Forms.Button();
            this.lbl_loggedIn = new System.Windows.Forms.Label();
            this.lbl_date = new System.Windows.Forms.Label();
            this.txt_month = new System.Windows.Forms.TextBox();
            this.txt_day = new System.Windows.Forms.TextBox();
            this.txt_year = new System.Windows.Forms.TextBox();
            this.lbl_slash1 = new System.Windows.Forms.Label();
            this.lbl_slash2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txt_fileDir
            // 
            this.txt_fileDir.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_fileDir.Location = new System.Drawing.Point(12, 67);
            this.txt_fileDir.Name = "txt_fileDir";
            this.txt_fileDir.Size = new System.Drawing.Size(325, 23);
            this.txt_fileDir.TabIndex = 3;
            this.txt_fileDir.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_fileDir_KeyDown);
            // 
            // fileBrowseButton
            // 
            this.fileBrowseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileBrowseButton.Location = new System.Drawing.Point(343, 67);
            this.fileBrowseButton.Name = "fileBrowseButton";
            this.fileBrowseButton.Size = new System.Drawing.Size(79, 23);
            this.fileBrowseButton.TabIndex = 4;
            this.fileBrowseButton.Text = "Browse";
            this.fileBrowseButton.UseVisualStyleBackColor = true;
            this.fileBrowseButton.Click += new System.EventHandler(this.fileBrowseButton_Click);
            // 
            // uploadButton
            // 
            this.uploadButton.Location = new System.Drawing.Point(281, 118);
            this.uploadButton.Name = "uploadButton";
            this.uploadButton.Size = new System.Drawing.Size(120, 35);
            this.uploadButton.TabIndex = 5;
            this.uploadButton.Text = "Upload!";
            this.uploadButton.UseVisualStyleBackColor = true;
            this.uploadButton.Click += new System.EventHandler(this.uploadButton_Click);
            // 
            // uploadProgress
            // 
            this.uploadProgress.Location = new System.Drawing.Point(12, 191);
            this.uploadProgress.MarqueeAnimationSpeed = 0;
            this.uploadProgress.Name = "uploadProgress";
            this.uploadProgress.Size = new System.Drawing.Size(410, 22);
            this.uploadProgress.Step = 1;
            this.uploadProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.uploadProgress.TabIndex = 20;
            // 
            // uploadBackgroundWorker
            // 
            this.uploadBackgroundWorker.WorkerReportsProgress = true;
            this.uploadBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.uploadBackgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.uploadBackgroundWorker_ProgressChanged);
            this.uploadBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.uploadBackgroundWorker_RunWorkerCompleted);
            // 
            // lbl_statusTxt
            // 
            this.lbl_statusTxt.AutoSize = true;
            this.lbl_statusTxt.Location = new System.Drawing.Point(11, 174);
            this.lbl_statusTxt.Name = "lbl_statusTxt";
            this.lbl_statusTxt.Size = new System.Drawing.Size(97, 13);
            this.lbl_statusTxt.TabIndex = 20;
            this.lbl_statusTxt.Text = "Parsing combat log";
            this.lbl_statusTxt.Visible = false;
            // 
            // txt_userName
            // 
            this.txt_userName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_userName.Location = new System.Drawing.Point(68, 9);
            this.txt_userName.Name = "txt_userName";
            this.txt_userName.Size = new System.Drawing.Size(90, 20);
            this.txt_userName.TabIndex = 0;
            // 
            // txt_pass
            // 
            this.txt_pass.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_pass.Location = new System.Drawing.Point(227, 9);
            this.txt_pass.Name = "txt_pass";
            this.txt_pass.Size = new System.Drawing.Size(90, 20);
            this.txt_pass.TabIndex = 1;
            this.txt_pass.UseSystemPasswordChar = true;
            this.txt_pass.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_pass_KeyDown);
            // 
            // lbl_pass
            // 
            this.lbl_pass.AutoSize = true;
            this.lbl_pass.Location = new System.Drawing.Point(171, 13);
            this.lbl_pass.Name = "lbl_pass";
            this.lbl_pass.Size = new System.Drawing.Size(56, 13);
            this.lbl_pass.TabIndex = 12;
            this.lbl_pass.Text = "Password:";
            // 
            // lbl_userName
            // 
            this.lbl_userName.AutoSize = true;
            this.lbl_userName.Location = new System.Drawing.Point(10, 13);
            this.lbl_userName.Name = "lbl_userName";
            this.lbl_userName.Size = new System.Drawing.Size(58, 13);
            this.lbl_userName.TabIndex = 13;
            this.lbl_userName.Text = "Username:";
            // 
            // loginButton
            // 
            this.loginButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loginButton.Location = new System.Drawing.Point(332, 8);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(90, 21);
            this.loginButton.TabIndex = 2;
            this.loginButton.Text = "Login";
            this.loginButton.UseVisualStyleBackColor = true;
            this.loginButton.Click += new System.EventHandler(this.loginButton_Click);
            // 
            // lbl_loggedIn
            // 
            this.lbl_loggedIn.AutoSize = true;
            this.lbl_loggedIn.ForeColor = System.Drawing.Color.Red;
            this.lbl_loggedIn.Location = new System.Drawing.Point(10, 41);
            this.lbl_loggedIn.Name = "lbl_loggedIn";
            this.lbl_loggedIn.Size = new System.Drawing.Size(75, 13);
            this.lbl_loggedIn.TabIndex = 15;
            this.lbl_loggedIn.Text = "Not Logged In";
            // 
            // lbl_date
            // 
            this.lbl_date.AutoSize = true;
            this.lbl_date.Location = new System.Drawing.Point(34, 129);
            this.lbl_date.Name = "lbl_date";
            this.lbl_date.Size = new System.Drawing.Size(70, 13);
            this.lbl_date.TabIndex = 21;
            this.lbl_date.Text = "Date of Raid:";
            // 
            // txt_month
            // 
            this.txt_month.Location = new System.Drawing.Point(110, 126);
            this.txt_month.Name = "txt_month";
            this.txt_month.Size = new System.Drawing.Size(25, 20);
            this.txt_month.TabIndex = 6;
            // 
            // txt_day
            // 
            this.txt_day.Location = new System.Drawing.Point(143, 126);
            this.txt_day.Name = "txt_day";
            this.txt_day.Size = new System.Drawing.Size(25, 20);
            this.txt_day.TabIndex = 7;
            // 
            // txt_year
            // 
            this.txt_year.Location = new System.Drawing.Point(176, 126);
            this.txt_year.Name = "txt_year";
            this.txt_year.Size = new System.Drawing.Size(40, 20);
            this.txt_year.TabIndex = 8;
            // 
            // lbl_slash1
            // 
            this.lbl_slash1.AutoSize = true;
            this.lbl_slash1.Location = new System.Drawing.Point(133, 129);
            this.lbl_slash1.Name = "lbl_slash1";
            this.lbl_slash1.Size = new System.Drawing.Size(12, 13);
            this.lbl_slash1.TabIndex = 25;
            this.lbl_slash1.Text = "/";
            // 
            // lbl_slash2
            // 
            this.lbl_slash2.AutoSize = true;
            this.lbl_slash2.Location = new System.Drawing.Point(166, 129);
            this.lbl_slash2.Name = "lbl_slash2";
            this.lbl_slash2.Size = new System.Drawing.Size(12, 13);
            this.lbl_slash2.TabIndex = 26;
            this.lbl_slash2.Text = "/";
            // 
            // riftLogsUploader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 227);
            this.Controls.Add(this.lbl_slash2);
            this.Controls.Add(this.lbl_slash1);
            this.Controls.Add(this.txt_year);
            this.Controls.Add(this.txt_day);
            this.Controls.Add(this.txt_month);
            this.Controls.Add(this.lbl_date);
            this.Controls.Add(this.lbl_loggedIn);
            this.Controls.Add(this.loginButton);
            this.Controls.Add(this.lbl_userName);
            this.Controls.Add(this.lbl_pass);
            this.Controls.Add(this.txt_pass);
            this.Controls.Add(this.txt_userName);
            this.Controls.Add(this.lbl_statusTxt);
            this.Controls.Add(this.uploadProgress);
            this.Controls.Add(this.uploadButton);
            this.Controls.Add(this.fileBrowseButton);
            this.Controls.Add(this.txt_fileDir);
            this.Name = "riftLogsUploader";
            this.Text = "RIFT Logs Uploader";
            this.Load += new System.EventHandler(this.riftLogsUploader_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_fileDir;
        private System.Windows.Forms.Button fileBrowseButton;
        private System.Windows.Forms.Button uploadButton;
        private System.Windows.Forms.ProgressBar uploadProgress;
        private System.ComponentModel.BackgroundWorker uploadBackgroundWorker;
        private System.Windows.Forms.Label lbl_statusTxt;
        private System.Windows.Forms.TextBox txt_userName;
        private System.Windows.Forms.TextBox txt_pass;
        private System.Windows.Forms.Label lbl_pass;
        private System.Windows.Forms.Label lbl_userName;
        private System.Windows.Forms.Button loginButton;
        private System.Windows.Forms.Label lbl_loggedIn;
        private System.Windows.Forms.Label lbl_date;
        private System.Windows.Forms.TextBox txt_month;
        private System.Windows.Forms.TextBox txt_day;
        private System.Windows.Forms.TextBox txt_year;
        private System.Windows.Forms.Label lbl_slash1;
        private System.Windows.Forms.Label lbl_slash2;
    }
}

