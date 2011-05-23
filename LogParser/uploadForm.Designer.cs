﻿namespace LogParser
{
    partial class riftParser
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
            this.SuspendLayout();
            // 
            // txt_fileDir
            // 
            this.txt_fileDir.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_fileDir.Location = new System.Drawing.Point(12, 46);
            this.txt_fileDir.Name = "txt_fileDir";
            this.txt_fileDir.Size = new System.Drawing.Size(325, 23);
            this.txt_fileDir.TabIndex = 0;
            // 
            // fileBrowseButton
            // 
            this.fileBrowseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileBrowseButton.Location = new System.Drawing.Point(343, 46);
            this.fileBrowseButton.Name = "fileBrowseButton";
            this.fileBrowseButton.Size = new System.Drawing.Size(79, 23);
            this.fileBrowseButton.TabIndex = 1;
            this.fileBrowseButton.Text = "Browse";
            this.fileBrowseButton.UseVisualStyleBackColor = true;
            this.fileBrowseButton.Click += new System.EventHandler(this.fileBrowseButton_Click);
            // 
            // uploadButton
            // 
            this.uploadButton.Location = new System.Drawing.Point(157, 76);
            this.uploadButton.Name = "uploadButton";
            this.uploadButton.Size = new System.Drawing.Size(120, 35);
            this.uploadButton.TabIndex = 2;
            this.uploadButton.Text = "Upload!";
            this.uploadButton.UseVisualStyleBackColor = true;
            this.uploadButton.Click += new System.EventHandler(this.uploadButton_Click);
            // 
            // uploadProgress
            // 
            this.uploadProgress.Location = new System.Drawing.Point(12, 131);
            this.uploadProgress.Name = "uploadProgress";
            this.uploadProgress.Size = new System.Drawing.Size(410, 22);
            this.uploadProgress.Step = 1;
            this.uploadProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.uploadProgress.TabIndex = 3;
            // 
            // riftParser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 162);
            this.Controls.Add(this.uploadProgress);
            this.Controls.Add(this.uploadButton);
            this.Controls.Add(this.fileBrowseButton);
            this.Controls.Add(this.txt_fileDir);
            this.Name = "riftParser";
            this.Text = "RIFT Logs Parser";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_fileDir;
        private System.Windows.Forms.Button fileBrowseButton;
        private System.Windows.Forms.Button uploadButton;
        private System.Windows.Forms.ProgressBar uploadProgress;
    }
}
