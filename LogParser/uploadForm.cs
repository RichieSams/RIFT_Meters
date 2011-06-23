//#define PARSE
//#define COMPRESS

using System;
using System.Collections;
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
        public riftLogsUploader()
        {
            InitializeComponent();
        }

        #region Variables

        // Spell Dictionary
        Dictionary<string, string> spellDict;

        // Entity Dictionary
        struct entityDef {
            public string id;
            public string name;
        }
        Dictionary<string, entityDef> entityDict;

        // Login
        Boolean loggedIn = false;

        // WebClient
        CookieAwareWebClient Client = new CookieAwareWebClient();
        string k;
        byte[] result;


        #endregion // Variables

        #region Log in

        private void loginFunction()
        {
            // Saving values of text boxes then clearing them
            String username = txt_userName.Text;
            String pass = txt_pass.Text;
            txt_userName.Text = "";
            txt_pass.Text = "";

            // Hashing of password for security
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] interHash = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(pass + username));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < interHash.Length; i++)
            {
                sb.Append(interHash[i].ToString("x2"));
            }
            string interStr = sb.ToString();
            byte[] finalHash = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(pass + interStr));
            sb.Clear();
            for (int i = 0; i < finalHash.Length; i++)
            {
                sb.Append(finalHash[i].ToString("x2"));
            }
            string passMD5 = sb.ToString();

            // Set values to pass to php file
            NameValueCollection nvcLoginInfo = new NameValueCollection();
            nvcLoginInfo.Add("username", username);
            nvcLoginInfo.Add("pass", passMD5);

            // Call php
            result = Client.UploadValues("http://personaguild.com/publicRiftLogs/login.php", nvcLoginInfo);
            k = Encoding.UTF8.GetString(result, 0, result.Length);

            // Check echos
            String[] returnArgs = k.Split(',');
            switch (returnArgs[0])
            {
                case "true":
                    loggedIn = true;
                    lbl_loggedIn.Text = "Logged in as " + username;
                    lbl_loggedIn.ForeColor = Color.Green;
                    txt_fileDir.Focus();
                    break;
                case "false":
                    MessageBox.Show("Invalid login information", "Login failed");
                    break;

                case "FAILURE":
                    MessageBox.Show("login.php threw an error. Please contact an administrator with the circumstances that caused this error.", "PHP file error");
                    break;
            }
        }

        private void txt_pass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                loginFunction();
                e.SuppressKeyPress = true;
            }
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            loginFunction();
        }

        #endregion // Log in

        #region Layout

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
                uploadButton.Focus();
            }

        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            startWork();
        }

        private void txt_fileDir_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                startWork();
                e.SuppressKeyPress = true;
            }
        }

        #endregion // Layout

        #region Start Work

        private void startWork()
        {
            if (loggedIn)
            {
                if (txt_fileDir.Text != "")
                {
                    // Save directory so as to prevent errors if the user manages to change the text while the parsing is running
                    String logDir = txt_fileDir.Text;

                    // Prevent index errors on later file verification
                    if (logDir.Length >= 4)
                    {
                        // File verification
                        if (logDir.Substring(logDir.Length - 4) == ".txt")
                        {
                            // Turn on the status text
                            lbl_statusTxt.Show();

                            // Start work
                            uploadBackgroundWorker.RunWorkerAsync(logDir);
                        }
                        else
                        {
                            MessageBox.Show("Entry must be a .txt file. Please enter a valid file", "Incorrect file type");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid entry", "Invalid Entry");
                    }
                }
                else
                {
                    MessageBox.Show("No file specified", "No file");
                }
            }
            else
            {
                MessageBox.Show("You must log in before uploading", "Not logged in");
            }
        }

        #endregion // Start Work

        #region Work

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            String logDir = (String)e.Argument;
            double progress = 0;
            int lineCount = File.ReadLines(logDir).Count();
            spellDict = new Dictionary<string, string>();
            entityDict = new Dictionary<string, entityDef>();

#if PARSE

            #region Parsing

            try
            {
                // Open combat log file
                FileStream fs = new FileStream(logDir, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(fs);

                // Create csv file and the file writer
                TextWriter dataWriter = new StreamWriter("data.csv");
                String startTime = null;

                // Create encounter variables
                ArrayList encArray = new ArrayList();
                Boolean fighting = false;
                int encNum = 1;
                DateTime encStart = new DateTime();
                DateTime encEnd = new DateTime();
                String npcName = string.Empty;

                String line = string.Empty;
                int lineCounter = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    lineCounter++;
                    // Initiate the data containers
                    String SourceID = "\\N";
                    String SourceName = "\\N";
                    String SourceType = string.Empty;
                    String TargetID = "\\N";
                    String TargetName = "\\N";
                    String TargetType = string.Empty;
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
                            SourceType = CodeList[1].Split('#')[0];
                            if (!((s = CodeList[2].Split('#')[2]).Equals("0"))) TargetID = s;
                            if (!((s = CodeList[2].Split('#')[0]).Equals("T=X"))) TargetType = s;
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

                            // If the source is a non-pet npc and the target is a player, start an encounter
                            if ((!fighting) && (SourceType == "T=N") && (TargetType == "T=P") && (SourceOwnerID == "\\N"))
                            {
                                fighting = true;
                                npcName = SourceName;
                                encStart = DateTime.Parse(Time);
                                encEnd = encStart.AddSeconds(10);
                            }

                            // If the souce is a player and the target is a non-pet npc, start an encounter
                            if ((!fighting) && (SourceType == "T=P") && (TargetType == "T=N") && (TargetOwnerID == "\\N"))
                            {
                                fighting = true;
                                npcName = TargetName;
                                encStart = DateTime.Parse(Time);
                                encEnd = encStart.AddSeconds(10);
                            }

                            if (fighting)
                            {
                                // Add the row to the array
                                encArray.Add(Time + "," + TypeID + "," + SourceID + "," + TargetID + "," + SpellID + "," + Amount + "," + Element + "," + AbsorbedValue + "," + BlockedValue + "," + OverhealValue + "," + OverkillValue + ",");
                                // If the npc is a target or source, extend the end time by 10 seconds
                                if (!((TargetName != npcName) && (SourceName != npcName))) encEnd = DateTime.Parse(Time).AddSeconds(10);

                                // If 10 seconds go by before there is any action by the npc, the encounter ends
                                if (DateTime.Parse(Time) == encEnd)
                                {
                                    fighting = false;
                                    String tempEncNum = "\\N";
                                    
                                    // Only use the encounter if it is longer than 30 seconds
                                    if (encEnd - encStart > TimeSpan.FromSeconds(30))
                                    {
                                        tempEncNum = encNum.ToString();
                                        encNum++;
                                    }

                                    // Write each line to the csv file
                                    foreach (String row in encArray)
                                    {
                                        dataWriter.WriteLine(row + tempEncNum + ",");
                                    }

                                    // Clear the array
                                    encArray.Clear();
                                    
                                }
                            }
                            else
                            {
                                // Write the data to the csv file
                                dataWriter.WriteLine(Time + ",\\N," + TypeID + "," + SourceID + "," + TargetID + "," + SpellID + "," + Amount + "," + Element + "," + AbsorbedValue + "," + BlockedValue + "," + OverhealValue + "," + OverkillValue + ",");
                            }
                        }
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

            // Update progress
            uploadBackgroundWorker.ReportProgress(20);

            #endregion // Parsing region

#endif // PARSE

#if COMPRESS

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

#endif // COMPRESS

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

            #region FTP Upload

            progress = 0;
            // Create ftp object
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://ftp.personaguild.com/"+md5hash+".zip");
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // Input credentials
            request.Credentials = new NetworkCredential(Properties.Settings.Default.FTPAccount, Properties.Settings.Default.FTPPassword); // Need to change this to make it secure

            // Set paramaters
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = true;
            request.ReadWriteTimeout = 10000000;

            // Read the file to a buffer
            FileStream ftpFs = File.OpenRead("temp.zip");
            byte[] buffer = new byte[ftpFs.Length];
            ftpFs.Read(buffer, 0, buffer.Length);
            ftpFs.Close();

            // Stream chunks of the buffer to ftp
            Stream ftpstream = request.GetRequestStream();
            int bufferPart = 0;
            int bufferLength = buffer.Length;
            int chunkSize = 102400;
            double numChunks = Math.Ceiling((double)bufferLength / chunkSize);
            while (bufferPart < bufferLength)
            {
                if ((bufferLength - bufferPart) >= chunkSize) // Need to fiddle with this to minimize time, but prevent the server from cutting the connection
                {
                    ftpstream.Write(buffer, bufferPart, chunkSize);
                }
                else
                {
                    ftpstream.Write(buffer, bufferPart, bufferLength - bufferPart);
                }
                bufferPart += chunkSize;

                uploadBackgroundWorker.ReportProgress((int)(40 + ((++progress / numChunks) * 20)));
            }
            ftpstream.Close();

            #endregion // FTP Upload

            
            NameValueCollection nvcDecompress = new NameValueCollection();
            nvcDecompress.Add("file", md5hash);

            #region Check File Integrity

            result = Client.UploadValues("http://personaguild.com/publicRiftLogs/check.php", nvcDecompress);
            k = Encoding.UTF8.GetString(result, 0, result.Length);

            // File was corrupted during upload
            if (!md5hash.Equals(k))
            {
                MessageBox.Show("File was corrupted during upload. Retry uploading", "File corrupted");
                return;
            }

            #endregion // Check File Integrity

            #region Decompress

            //Client.Headers.Remove("Content-Type");

            result = Client.UploadValues("http://personaguild.com/publicRiftLogs/decompress.php", nvcDecompress);
            k = Encoding.UTF8.GetString(result, 0, result.Length);

            // Update progress
            uploadBackgroundWorker.ReportProgress(80);

            #endregion // Decompress

            #region Insert
            //string offset = TimeZone.CurrentTimeZone.GetUtcOffset(TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now)).Hours.ToString();

            //nvcDecompress.Add("timezone", offset);

            result = Client.UploadValues("http://personaguild.com/publicRiftLogs/insert.php", nvcDecompress);
            k = Encoding.UTF8.GetString(result, 0, result.Length);

            // Update progress
            uploadBackgroundWorker.ReportProgress(100);

            #endregion // Insert

            return;

        }

        #endregion // Work

        #region Progress

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
                    lbl_statusTxt.Text = "Uploading data - " + (e.ProgressPercentage - 40) * 5 + "% (This can take some time)";
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

        #endregion // Progress

    }

    #region Cookie Class

    public class CookieAwareWebClient : WebClient
    {

        private CookieContainer m_container = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = m_container;
            }
            return request;
        }
    }

    #endregion // Cookie Class

}
