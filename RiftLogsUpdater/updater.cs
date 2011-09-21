using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using System.Threading;

namespace RiftLogsUpdater
{
    public partial class updater : Form
    {
        public updater()
        {
            InitializeComponent();
        }

        WebClient client = new WebClient();

        private void updater_Load(object sender, EventArgs e)
        {
            // Make sure LogParser.exe is completely closed before updating
            while (Process.GetProcessesByName("LogParser").Length > 0)
            {
                Thread.Sleep(100);
            }

            // Download the new files
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadCompleted);
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadProgressChanged);
            client.DownloadFileAsync(new Uri("http://www.personaguild.com/publicRiftLogs/update.zip"), "update.zip");
        }

        private void downloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage != updateProgress.Value)
            {
                updateProgress.Value = e.ProgressPercentage;
            }
        }

        private void downloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            updateProgress.Value = 0;
            this.Text = "Updating";
            updaterBackgroundWorker.RunWorkerAsync();
        }

        private void updaterBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Decompress
            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(Application.StartupPath+"\\update.zip");
                zf = new ZipFile(fs);
                int interval = (int)(100 / zf.Count);
                foreach (ZipEntry zipEntry in zf)
                {
                    String entryFileName = zipEntry.Name;

                    byte[] buffer = new byte[4096];
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    String fullZipToPath = Path.Combine(Application.StartupPath + "\\update", entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                    
                    // Progress
                    updateProgress.Value += interval;
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close();
                }
            }

            // File replacing
            if (File.Exists(Path.Combine(Application.StartupPath, "LogParser.exe")))
            {
                File.Delete(Path.Combine(Application.StartupPath, "LogParser.exe"));
            }
            File.Move(Path.Combine(Application.StartupPath + "\\update", "LogParser.exe"), Path.Combine(Application.StartupPath, "LogParser.exe"));
            File.Move(Path.Combine(Application.StartupPath, "ICSharpCode.SharpZipLib.dll"), Path.Combine(Application.StartupPath, "old-zip.dll"));
            File.Move(Path.Combine(Application.StartupPath + "\\update", "ICSharpCode.SharpZipLib.dll"), Path.Combine(Application.StartupPath, "ICSharpCode.SharpZipLib.dll"));
            File.Move(Path.Combine(Application.StartupPath + "\\update", "RiftLogsUpdater.exe"), Path.Combine(Application.StartupPath, "RiftLogsUpdater.exe.new"));    

            // Clean up
            Directory.Delete(Application.StartupPath + "\\update", true);
            File.Delete(Application.StartupPath + "\\update.zip");
        }

        private void updaterBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage != updateProgress.Value)
            {
                updateProgress.Value = e.ProgressPercentage;
            }
        }

        private void updaterBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Re open LogParser and close the updater
            Process process = new Process();
            process.StartInfo.FileName = Path.Combine(Application.StartupPath, "LogParser.exe");
            process.Start();
            Application.Exit();
        }
    }
}
