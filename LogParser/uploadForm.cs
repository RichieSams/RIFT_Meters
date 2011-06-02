using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography;
using System.Collections.Specialized;

namespace LogParser
{
    public partial class riftLogsUploader : Form
    {

        // Spell Dictionary
        Dictionary<string, string> spellDict;

        // Entity Dictionary
        struct entityDef {
            public string id;
            public string name;
        }
        Dictionary<string, entityDef> entityDict;
        

        public riftLogsUploader()
        {
            InitializeComponent();
        }

        private void fileBrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt";
            ofd.FilterIndex = 1;
            ofd.Multiselect = false;
            ofd.AddExtension = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txt_fileDir.Text = ofd.FileName;
            }

        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            // Save directory so as to prevent errors if the user manages to change the text while the parsing is running
            String logDir = txt_fileDir.Text;

            // Turn on the status text
            lbl_statusTxt.Show();

            // Start work
            uploadBackgroundWorker.RunWorkerAsync(logDir);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            String logDir = (String)e.Argument;
            double progress = 0;
            int lineCount = File.ReadLines(logDir).Count();
            spellDict = new Dictionary<string, string>();
            entityDict = new Dictionary<string, entityDef>();

            #region Parsing

            try
            {
                // Open combat log file
                FileStream fs = new FileStream(logDir, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(fs);

                // Create csv file and the file writer
                TextWriter dataWriter = new StreamWriter("data.csv");
                String startTime = null;

                String line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    // Initiate the data containers
                    String SourceID = "\\N";
                    String SourceName = "\\N";
                    String TargetID = "\\N";
                    String TargetName = "\\N";
                    String SourceOwnerID = "\\N";
                    String TargetOwnerID = "\\N";
                    String Amount = "\\N";
                    String TypeID = "\\N";
                    String SpellID = "\\N";
                    String SpellName = "\\N";
                    String Time = "\\N";
                    String Element = "\\N";
                    String BlockedValue = "\\N";
                    String OverkillValue = "\\N";
                    String OverhealValue = "\\N";
                    String AbsorbedValue = "\\N";

                    if (line != "")
                    {
                        // Get time
                        Time = line.Substring(0, 8);
                        if (startTime == null)
                        {
                            startTime = Time;
                            dataWriter.WriteLine(startTime);
                        }
                    }

                    // Only parse combat lines
                    if (line.IndexOf(")") > 0)
                    {
                        // Break into code and words
                        string CodeStr = line.Substring(11, line.IndexOf(")") - 1);
                        string LogStr = line.Substring(line.IndexOf(")") + 1, line.Length - (line.IndexOf(")") + 1));

                        // Split code string into parts and trim off blanks
                        string[] CodeList = CodeStr.Split(',');
                        for (int i = 0; i <= 9; i++)
                        {
                            CodeList[i] = CodeList[i].Trim();
                        }

                        // Only parse relevant combat lines
                        if (((int.Parse(CodeList[0]) >= 1) && (int.Parse(CodeList[0]) <= 23)) || ((int.Parse(CodeList[0]) >= 26) && (int.Parse(CodeList[0]) <= 28)))
                        {
                            // Set ID's and names
                            string s;
                            TypeID = CodeList[0];
                            SourceID = CodeList[1].Split('#')[2];
                            if (!((s = CodeList[2].Split('#')[2]).Equals("0"))) TargetID = s;
                            if (!((s = CodeList[3].Split('#')[2]).Equals("0"))) SourceOwnerID = s;
                            if (!((s = CodeList[4].Split('#')[2]).Equals("0"))) TargetOwnerID = s;
                            SourceName = CodeList[5];
                            if (!((s = CodeList[6]).Equals("Unknown")))
                            {
                                TargetName = s;
                            } 
                            if (((int.Parse(CodeList[0]) >= 3) && (int.Parse(CodeList[0]) <= 5)) || (int.Parse(CodeList[0]) == 14) || (int.Parse(CodeList[0]) == 23) || ((int.Parse(CodeList[0]) >= 27) && (int.Parse(CodeList[0]) <= 28)))
                                Amount = CodeList[7];
                            if (!((s = CodeList[8]).Equals("0")))
                            {
                                SpellID = s;
                                SpellName = CodeList[9].Split(')')[0].Trim();
                                // Generate Spell Dict
                                if (!spellDict.ContainsKey(SpellID) && !SpellName.Equals(""))
                                    spellDict.Add(SpellID, SpellName);
                            }

                            // Get Element
                            Regex r = new Regex("\\d (\\w+) damage");
                            Match m = r.Match(LogStr);
                            if (m.Success)
                            {
                                GroupCollection g = m.Groups;
                                Element = g[1].Captures[0].Value;
                            }

                            // Generate Entity
                            if (!entityDict.ContainsKey(SourceID))
                            {
                                entityDef ent = new entityDef();
                                if (!SourceOwnerID.Equals("\\N"))
                                    ent.id = SourceOwnerID;
                                else
                                    ent.id = null;
                                ent.name = SourceName;
                                entityDict.Add(SourceID, ent);
                            }
                            if (!TargetID.Equals("\\N"))
                            {
                                if (!entityDict.ContainsKey(TargetID))
                                {
                                    entityDef ent = new entityDef();
                                    if (!TargetOwnerID.Equals("\\N"))
                                        ent.id = TargetOwnerID;
                                    else
                                        ent.id = null;
                                    ent.name = TargetName;
                                    entityDict.Add(TargetID, ent);
                                }
                            }
                            // Check for special cases
                            if (LogStr.IndexOf("(") != -1)
                            {
                                string[] AddInfo = LogStr.Substring(LogStr.LastIndexOf('(') + 1, LogStr.LastIndexOf(')') - LogStr.LastIndexOf('(') - 1).Split(' ');
                                if (AddInfo.GetUpperBound(0) > 0)
                                {

                                    for (int j = 0; j <= AddInfo.GetUpperBound(0); j = j + 2)
                                    {

                                        switch (AddInfo[j + 1].Trim().ToLower())
                                        {
                                            case "absorbed":
                                                AbsorbedValue = AddInfo[j];
                                                break;
                                            case "blocked":
                                                BlockedValue = AddInfo[j];
                                                break;
                                            case "overheal":
                                                OverhealValue = AddInfo[j];
                                                break;
                                            case "overkill":
                                                OverkillValue = AddInfo[j];
                                                break;
                                        }
                                    }
                                }
                            }
                            // Write the data to the csv file

                            dataWriter.WriteLine(Time + "," + TypeID + "," + SourceID + "," + TargetID + "," + SpellID + "," + Amount + "," + Element + "," + AbsorbedValue + "," + BlockedValue + "," + OverhealValue + "," + OverkillValue + ",");
                        }
                    }
                    else if (line.Contains("Combat Begin"))
                    {
                        TypeID = "29";
                        dataWriter.WriteLine(Time + "," + TypeID + "," + SourceID + "," + TargetID + "," + SpellID + "," + Amount + "," + Element + "," + AbsorbedValue + "," + BlockedValue + "," + OverhealValue + "," + OverkillValue + ",");
                    }
                    else if (line.Contains("Combat End"))
                    {
                        TypeID = "30";
                        dataWriter.WriteLine(Time + "," + TypeID + "," + SourceID + "," + TargetID + "," + SpellID + "," + Amount + "," + Element + "," + AbsorbedValue + "," + BlockedValue + "," + OverhealValue + "," + OverkillValue + ",");
                    }

                    uploadBackgroundWorker.ReportProgress((int)((++progress / lineCount) * 20));

                }
                // Close the csv file
                dataWriter.Close();
            }
            catch (IOException)
            {
                MessageBox.Show("File does not exist or was entered incorrectly. Please enter a file or browse to it, then retry.", "Incorrect file");
                return;
            }

            try
            {
                TextWriter spellWriter = new StreamWriter("spell.csv");

                foreach (KeyValuePair<string, string> kvp in spellDict) 
                {
                    spellWriter.WriteLine(kvp.Key + "," + kvp.Value + ",");
                }

                spellWriter.Close();
            }
            catch (IOException)
            {

            }

            try
            {
                TextWriter entityWriter = new StreamWriter("entity.csv");

                string s = string.Empty;

                foreach (KeyValuePair<string, entityDef> kvp in entityDict)
                {
                    string owner = "\\N";
                    if (kvp.Value.id != null) owner = kvp.Value.id;
                    entityWriter.WriteLine(kvp.Key + "," + owner + "," + kvp.Value.name + ",");
                }

                entityWriter.Close();
            }
            catch (IOException)
            {

            }



            #endregion // Parsing region

            #region Compression

            string fnOut = @"temp.zip";
            ZipOutputStream zipStream = new ZipOutputStream(File.Create(fnOut));
            zipStream.SetLevel(9); //0-9, 9 being the highest level of compression
            //zipStream.Password = "ok";

            string[] files = { @"data.csv", @"spell.csv", @"entity.csv" };
            foreach (string inputFn in files)
            {
                FileInfo fi = new FileInfo(inputFn);
                string entryName = ZipEntry.CleanName(inputFn);
                ZipEntry newEntry = new ZipEntry(entryName);

                zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);
                byte[] buff = new byte[4096];
                using (FileStream streamReader = File.OpenRead(inputFn))
                {
                    StreamUtils.Copy(streamReader, zipStream, buff);
                }
                zipStream.CloseEntry();
            }
            zipStream.IsStreamOwner = true;	// Makes the Close also Close the underlying stream
            zipStream.Close();

            #endregion // Compression

            #region MD5Hash

            FileStream file = new FileStream(@"temp.zip", FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            string md5hash = sb.ToString();

            // Update progress
            uploadBackgroundWorker.ReportProgress(40);

            #endregion

            #region PHP Upload

            WebClient Client = new WebClient();
            Client.Headers.Add("Content-Type", "binary/octet-stream");

            byte[] result = Client.UploadFile("http://personaguild.com/publicRiftLogs/upload.php", "POST", "temp.zip");
            string k = Encoding.UTF8.GetString(result, 0, result.Length);

            // File was corrupted during upload
            if (!md5hash.Equals(k))
            {
                MessageBox.Show("File was corrupted during upload. Retry uploading", "File corrupted");
                return;
            }

            // Update progress
            uploadBackgroundWorker.ReportProgress(60);

            #endregion // PHP Upload

            #region Decompress

            Client.Headers.Remove("Content-Type");
            NameValueCollection nvcDecompress = new NameValueCollection();
            nvcDecompress.Add("file", md5hash);

            result = Client.UploadValues("http://personaguild.com/publicRiftLogs/decompress.php", nvcDecompress);
            k = Encoding.UTF8.GetString(result, 0, result.Length);

            // Update progress
            uploadBackgroundWorker.ReportProgress(80);

            #endregion // Decompress

            #region Insert

            result = Client.UploadValues("http://personaguild.com/publicRiftLogs/insert.php", nvcDecompress);
            k = Encoding.UTF8.GetString(result, 0, result.Length);

            #endregion // Insert

            // Update progress
            uploadBackgroundWorker.ReportProgress(100);
            return;

        }

        private void uploadBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage != uploadProgress.Value)
            {
                uploadProgress.Value = e.ProgressPercentage;

                if (e.ProgressPercentage < 20)
                {
                    lbl_statusTxt.Text = "Parsing combat log - " + e.ProgressPercentage*5 + "%";
                }
                else if (e.ProgressPercentage < 40)
                {
                    lbl_statusTxt.Text = "Compressing data";
                }
                else if (e.ProgressPercentage < 60)
                {
                    lbl_statusTxt.Text = "Uploading data (This can take some time)";
                }
                else if (e.ProgressPercentage < 80)
                {
                    lbl_statusTxt.Text = "Decompressing data";
                }
                else
                {
                    lbl_statusTxt.Text = "Inserting data into the database";
                }
            }           
        }

        private void uploadBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lbl_statusTxt.Text = "Done";
            MessageBox.Show("Done!!");
        }

    }
}
