using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;

namespace LogParser
{
    public partial class riftLogsUploader : Form
    {
        public riftLogsUploader()
        {
            InitializeComponent();
        }

        #region Variables

        // Version
        Int16 version = 10004;

        // Upload success
        Boolean done = false;

        // NPC Struct
        struct lastNPCTime
        {
            public DateTime time;
            public int index;

            public lastNPCTime(DateTime time, int index)
            {
                this.time = time;
                this.index = index;
            }
        }

        // Spell Dictionary
        Dictionary<string, string> spellDict;

        // Entity Dictionary
        struct entityDef {
            public string id;
            public string name;
        }
        Dictionary<string, entityDef> entityDict;

        // Raid Dictionary
        struct raidDef
        {
            public string id;
            public string time;
        }
        Dictionary<int, raidDef> raidDict;

        // Login
        Boolean loggedIn = false;

        // Creates a TextInfo based on the "en-US" culture.
        TextInfo usTxtInfo = new CultureInfo("en-US", false).TextInfo;

        // WebClient
        CookieAwareWebClient Client = new CookieAwareWebClient();
        string k;
        byte[] result;

        // IDs
        UInt16 playerID = 1; // 1-3000
        UInt16 playerPetID = 3001; // 3001-5000
        UInt16 npcID = 5001; // 5001-10000
        UInt16 npcPetID = 10001; // 10001-12000
        Dictionary<string, UInt16> ids;

        // Bosses
        String[] GSBbosses= {"Duke Letareus", "Infiltrator Johlen", "Oracle Aleria", "Prince Hylas", "Lord Greenscale"};
        String[] ROSbosses= {"Dark Focus", "Warmaster Galenir", "Plutonus the Immortal", "Herald Gaurath", "Alsbeth the Discordant"};
        String[] GPbosses= {"Anrak the Foul", "Guurloth", "Thalguur", "Uruluuk"};
        String[] HKbosses= {"Murdantix", "Soulrender Zilas", "Vladmal Prime", "Grugonim", "Rune King Molinar", "Prince Dollin", "Estrode", "Matron Zamira", "Sicaron", "Inquistor Garau", "Inwar Darktide", "Lord Jornaru", "Akylios"};
        String[] DHbosses = {"Assault Commander Jorb", "Joloral Ragetide", "Isskal", "High Priestess Hydriss"};

        // Timers
        Stopwatch parseTimer = new Stopwatch();

        #endregion // Variables

        #region On load

        private void riftLogsUploader_Load(object sender, EventArgs e)
        {
            try
            {
                WebClient updateClient = new WebClient();
                updateClient.Proxy = null;
                Stream stream = updateClient.OpenRead("http://www.riftmeters.com/version.ini");
                StreamReader sr = new StreamReader(stream);
                Int16 newVersion = Convert.ToInt16(sr.ReadToEnd());
                stream.Close();
                if (newVersion > version)
                {
                    Process process = new Process();
                    process.StartInfo.FileName = Path.Combine(Application.StartupPath, "RiftLogsUpdater.exe");
                    process.Start();
                    Application.Exit();
                }
                if (File.Exists(Path.Combine(Application.StartupPath, "old-zip.dll")))
                {
                    File.Delete(Path.Combine(Application.StartupPath, "old-zip.dll"));
                }
                if (File.Exists(Path.Combine(Application.StartupPath, "RiftLogsUpdater.exe.new")))
                {
                    if (File.Exists(Path.Combine(Application.StartupPath, "RiftLogsUpdater.exe")))
                    {
                        File.Delete(Path.Combine(Application.StartupPath, "RiftLogsUpdater.exe"));
                    }
                    File.Move(Path.Combine(Application.StartupPath, "RiftLogsUpdater.exe.new"), Path.Combine(Application.StartupPath, "RiftLogsUpdater.exe"));
                }
            }
            catch
            {
                MessageBox.Show("Rift Meters Client requires an internet connection. Connect to the internet,\n\t\t     then restart the program.", "Internet connection failed");
                Application.Exit();
            }

            txt_month.Text = DateTime.Today.Month.ToString();
            txt_day.Text = DateTime.Today.Day.ToString();
            txt_year.Text = DateTime.Today.Year.ToString();

            Client.Proxy = null;
        }

        #endregion // On load

        #region Log in

        private void loginFunction()
        {
            // Saving values of text boxes then clearing them
            String username = txt_userName.Text;
            String pass = txt_pass.Text;
            txt_userName.Text = "";
            txt_pass.Text = "";

            // Hashing of password for security
            SHA256 sha = new SHA256CryptoServiceProvider();
            byte[] interHash = sha.ComputeHash(System.Text.Encoding.Default.GetBytes(pass + username));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < interHash.Length; i++)
            {
                sb.Append(interHash[i].ToString("x2"));
            }
            string interStr = sb.ToString();
            byte[] finalHash = sha.ComputeHash(System.Text.Encoding.Default.GetBytes(pass + interStr));
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
            try
            {
                result = Client.UploadValues("http://www.riftmeters.com/login.php", nvcLoginInfo);
                k = Encoding.UTF8.GetString(result, 0, result.Length);

                // Check echos
                String[] returnArgs = k.Split(',');
                switch (returnArgs[0])
                {
                    case "true":
                        loggedIn = true;
                        lbl_loggedIn.Text = "Logged in as " + usTxtInfo.ToTitleCase(username);
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
            catch (System.Net.WebException)
            {
                MessageBox.Show("You must be connected to the internet to use the Rift Logs Uploader", "No internet connection");
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
                            // Disable editing while uploading
                            txt_pass.Enabled = false;
                            txt_userName.Enabled = false;
                            loginButton.Enabled = false;
                            txt_fileDir.Enabled = false;
                            fileBrowseButton.Enabled = false;
                            txt_month.Enabled = false;
                            txt_day.Enabled = false;
                            txt_year.Enabled = false;
                            uploadButton.Enabled = false;

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

        private String getID(String strOrigID, String strOwnerID, string name)
        {
            UInt64 origID = UInt64.Parse(strOrigID);
            UInt64 ownerID = UInt64.Parse(strOwnerID);
            string key = String.Empty;
            if (ownerID == 0)
                key = origID.ToString();
            else
                key = ownerID.ToString() + name;
            // Return null if the id is originally zero
            if (origID == 0)
            {
                return String.Empty;
            }
            else
            {
                // Check if it is a pet
                if (ownerID == 0)
                {
                    // Check if it is a npc or a player
                    if (origID > 8000000000000000000)
                    {
                        if (!(ids.ContainsKey(key)))
                        {
                            ids.Add(key, npcID++);
                        }
                        return ids[key].ToString();
                    }
                    else
                    {
                        if (!(ids.ContainsKey(key)))
                        {
                            ids.Add(key, playerID++);
                        }
                        return ids[key].ToString();
                    }
                }
                else
                {
                    // Check if it is a npc or a player
                    if (ownerID > 8000000000000000000)
                    {
                        if (!(ids.ContainsKey(key)))
                        {
                            ids.Add(key, npcPetID++);
                        }
                        return ids[key].ToString();
                    }
                    else
                    {
                        if (!(ids.ContainsKey(key)))
                        {
                            ids.Add(key, playerPetID++);
                        }
                        return ids[key].ToString();
                    }
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            parseTimer.Reset();
            parseTimer.Start();

            #region Parsing

            double progress = 0;
            int raidNum = 1;
            int raidID = 0;

            spellDict = new Dictionary<string, string>();
            entityDict = new Dictionary<string, entityDef>();
            ids = new Dictionary<string, UInt16>();
            raidDict = new Dictionary<int, raidDef>();


            try
            {
                String logDir = (String)e.Argument;
                int lineCount = File.ReadLines(logDir).Count();

                // Open combat log file
                FileStream fs = new FileStream(logDir, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(fs);

                // Create csv file and the file writer
                TextWriter dataWriter = new StreamWriter("data.csv");
                String startTime = null;
                DateTime startDateTime = new DateTime();

                String line = string.Empty;
                int lineCounter = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    lineCounter++;
                    
                    // Initiate the data containers
                    String SourceID = String.Empty;
                    String SourceName = String.Empty;
                    String TargetID = String.Empty;
                    String TargetName = String.Empty;
                    String SourceOwnerID = String.Empty;
                    String TargetOwnerID = String.Empty;
                    String Amount = String.Empty;
                    String TypeID = String.Empty;
                    String SpellID = String.Empty;
                    String SpellName = String.Empty;
                    String Time = String.Empty;
                    String Element = String.Empty;
                    String BlockedValue = String.Empty;
                    String OverkillValue = String.Empty;
                    String OverhealValue = String.Empty;
                    String AbsorbedValue = String.Empty;

                    if (line != "")
                    {
                        // Get time
                        Time = line.Substring(0, 8);
                        if (startTime == null)
                        {
                            startTime = Time;
                            startDateTime = DateTime.Parse(startTime);
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
                        if (((Int16.Parse(CodeList[0]) >= 1) && (Int16.Parse(CodeList[0]) <= 23)) || ((Int16.Parse(CodeList[0]) >= 26) && (Int16.Parse(CodeList[0]) <= 28)))
                        {
                            // Set names
                            string s;
                            SourceName = CodeList[5];
                            if (!((s = CodeList[6]).Equals("Unknown")))
                            {
                                TargetName = s;
                            }

                            // Set ID's
                            TypeID = CodeList[0];
                            // Generate new IDs
                            SourceID = getID(CodeList[1].Split('#')[2], CodeList[3].Split('#')[2], SourceName);
                            TargetID = getID(CodeList[2].Split('#')[2], CodeList[4].Split('#')[2], TargetName);
                            SourceOwnerID = getID(CodeList[3].Split('#')[2], "0", "");
                            TargetOwnerID = getID(CodeList[4].Split('#')[2], "0", "");

                            // Set Amount
                            if (((int.Parse(CodeList[0]) >= 3) && (int.Parse(CodeList[0]) <= 5)) || (int.Parse(CodeList[0]) == 14) || (int.Parse(CodeList[0]) == 23) || ((int.Parse(CodeList[0]) >= 27) && (int.Parse(CodeList[0]) <= 28)))
                                Amount = CodeList[7];
                            // Set spell ID
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
                                if (!SourceOwnerID.Equals(String.Empty))
                                    ent.id = SourceOwnerID;
                                else
                                    ent.id = String.Empty;
                                ent.name = SourceName;
                                entityDict.Add(SourceID, ent);
                            }
                            if (!TargetID.Equals(String.Empty))
                            {
                                if (!entityDict.ContainsKey(TargetID))
                                {
                                    entityDef ent = new entityDef();
                                    if (!TargetOwnerID.Equals(String.Empty))
                                        ent.id = TargetOwnerID;
                                    else
                                        ent.id = String.Empty;
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

                            raidDef tempRaid = new raidDef();
                            // Check for raid bosses to determine what instance
                            if (GSBbosses.Contains(SourceName) || GSBbosses.Contains(TargetName))
                            {
                                if (raidID!=1)
                                {
                                    if (raidID == 0)
                                    {
                                        raidID = 1;
                                        tempRaid.id = "1";
                                        tempRaid.time = startTime;
                                        raidDict.Add(raidNum, tempRaid);
                                    }
                                    else
                                    {
                                        raidNum++;
                                        raidID = 1;
                                        tempRaid.id = "1";
                                        tempRaid.time = Time;
                                        raidDict.Add(raidNum, tempRaid);
                                    }
                                }
                                    
                            }
                            else if (ROSbosses.Contains(SourceName) || ROSbosses.Contains(TargetName))
                            {
                                if (raidID != 2)
                                {
                                    if (raidID == 0)
                                    {
                                        raidID = 2;
                                        tempRaid.id = "2";
                                        tempRaid.time = startTime;
                                        raidDict.Add(raidNum, tempRaid);
                                    }
                                    else
                                    {
                                        raidNum++;
                                        raidID = 2;
                                        tempRaid.id = "2";
                                        tempRaid.time = Time;
                                        raidDict.Add(raidNum, tempRaid);
                                    }
                                }
                            }
                            else if (GPbosses.Contains(SourceName) || GPbosses.Contains(TargetName))
                            {
                                if (raidID != 3)
                                {
                                    if (raidID == 0)
                                    {
                                        raidID = 3;
                                        tempRaid.id = "3";
                                        tempRaid.time = startTime;
                                        raidDict.Add(raidNum, tempRaid);
                                    }
                                    else
                                    {
                                        raidNum++;
                                        raidID = 3;
                                        tempRaid.id = "3";
                                        tempRaid.time = Time;
                                        raidDict.Add(raidNum, tempRaid);
                                    }
                                }
                            }
                            else if (HKbosses.Contains(SourceName) || HKbosses.Contains(TargetName))
                            {
                                if (raidID != 4)
                                {
                                    if (raidID == 0)
                                    {
                                        raidID = 4;
                                        tempRaid.id = "4";
                                        tempRaid.time = startTime;
                                        raidDict.Add(raidNum, tempRaid);
                                    }
                                    else
                                    {
                                        raidNum++;
                                        raidID = 4;
                                        tempRaid.id = "4";
                                        tempRaid.time = Time;
                                        raidDict.Add(raidNum, tempRaid);
                                    }
                                }
                            }
                            else if (DHbosses.Contains(SourceName) || DHbosses.Contains(TargetName))
                            {
                                if (raidID != 5)
                                {
                                    if (raidID == 0)
                                    {
                                        raidID = 5;
                                        tempRaid.id = "5";
                                        tempRaid.time = startTime;
                                        raidDict.Add(raidNum, tempRaid);
                                    }
                                    else
                                    {
                                        raidNum++;
                                        raidID = 5;
                                        tempRaid.id = "5";
                                        tempRaid.time = Time;
                                        raidDict.Add(raidNum, tempRaid);
                                    }
                                }
                            }

                            // Write the data to the csv file
                            dataWriter.WriteLine(raidNum.ToString() + ">" + Time + "," + TypeID + "," + SourceID + "," + TargetID + "," + SpellID + "," + Amount + "," + Element + "," + AbsorbedValue + "," + BlockedValue + "," + OverhealValue + "," + OverkillValue + ",");

                        }
                    }

                    uploadBackgroundWorker.ReportProgress((int)((++progress / lineCount) * 33));

                }
                // Close the csv file
                dataWriter.Close();
            }
            catch (IOException)
            {
                MessageBox.Show("File does not exist or was entered incorrectly. Please enter a file or browse to it, then retry.", "Incorrect file");
                return;
            }

            if (raidID == 0)
            {
                MessageBox.Show("The combat log contains no raid bosses. Only upload raid combat logs", "No raid bosses found");
                return;
            }

            try
            {
                TextWriter defWriter = new StreamWriter("definitions.txt");
                defWriter.Write(txt_year.Text + "-" + txt_month.Text + "-" + txt_day.Text + "%");
                foreach (KeyValuePair<int, raidDef> kvp in raidDict)
                {
                    defWriter.Write(kvp.Key + ">" + kvp.Value.id + "," + kvp.Value.time + "*");
                }
                defWriter.Close();
            }
            catch (IOException)
            {

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
                    string owner = String.Empty;
                    if (kvp.Value.id != String.Empty) owner = kvp.Value.id;
                    entityWriter.WriteLine(kvp.Key + "," + owner + "," + kvp.Value.name + ",");
                }

                entityWriter.Close();
            }
            catch (IOException)
            {

            }

            parseTimer.Stop();
            //MessageBox.Show(parseTimer.ElapsedMilliseconds.ToString());

            // Update progress
            uploadBackgroundWorker.ReportProgress(33);

            #endregion // Parsing region

            #region Compression

            string fnOut = @"temp.zip";
            ZipOutputStream zipStream = new ZipOutputStream(File.Create(fnOut));
            zipStream.SetLevel(9); //0-9, 9 being the highest level of compression
            //zipStream.Password = "ok";

            string[] files = { @"data.csv", @"spell.csv", @"entity.csv", @"definitions.txt"};
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
            uploadBackgroundWorker.ReportProgress(66);

            #endregion

            #region FTP Upload

            progress = 0;
            // Create ftp object
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://ftp.riftmeters.com/"+md5hash+".zip");
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // Input credentials
            request.Credentials = new NetworkCredential("riftmeters@riftmeters.com", "[F_8bgoL0oth#HRsI2"); // Need to change this to make it secure

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

                uploadBackgroundWorker.ReportProgress((int)(66 + ((++progress / numChunks) * 33)));
            }
            ftpstream.Close();

            #endregion // FTP Upload
            
            NameValueCollection nvcDecompress = new NameValueCollection();
            nvcDecompress.Add("file", md5hash);

            #region Check File Integrity

            result = Client.UploadValues("http://www.riftmeters.com/check.php", nvcDecompress);
            k = Encoding.UTF8.GetString(result, 0, result.Length);

            // File was corrupted during upload
            if (!md5hash.Equals(k))
            {
                MessageBox.Show("File was corrupted during upload. Retry uploading", "File corrupted");
                return;
            }

            #endregion // Check File Integrity

            #region FileHandler

            result = Client.UploadValues("http://www.riftmeters.com/fileHandler.php", nvcDecompress);
            k = Encoding.UTF8.GetString(result, 0, result.Length);

            if (k != " SUCCESS")
            {
                MessageBox.Show("Data insertion failed. Retry upload. If problem persists, contact an administrator", "Data insertion failed");
                return;
            }

            // Update progress
            uploadBackgroundWorker.ReportProgress(100);

            #endregion // FileHandler

            // Reset ID incrementors and array
            playerID = 1; // 1-3000
            playerPetID = 3001; // 3001-5000
            npcID = 5001; // 5001-10000
            npcPetID = 10001; // 10001-12000

            done = true;
            return;

        }

        #endregion // Work

        #region Progress

        private void uploadBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage != uploadProgress.Value)
            {
                uploadProgress.Value = e.ProgressPercentage;
                if (e.ProgressPercentage < 33)
                {
                    lbl_statusTxt.Text = "Parsing combat log - " + e.ProgressPercentage*3 + "%";
                }
                else if (e.ProgressPercentage < 66)
                {
                    lbl_statusTxt.Text = "Compressing data";
                }
                else if (e.ProgressPercentage < 99)
                {
                    lbl_statusTxt.Text = "Uploading data - " + (e.ProgressPercentage - 66) * 3 + "%";
                }
                else
                {
                    lbl_statusTxt.Text = "Finishing";
                }
            }           
        }

        private void uploadBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (done)
            {
                lbl_statusTxt.Text = "Done";
                MessageBox.Show("Done! This raid should be viewable on the site in 1-3 minutes.", "Done uploading");
            }
            // Delete files
            File.Delete(Application.StartupPath + "\\data.csv");
            File.Delete(Application.StartupPath + "\\entity.csv");
            File.Delete(Application.StartupPath + "\\spell.csv");
            File.Delete(Application.StartupPath + "\\definitions.txt");
            File.Delete(Application.StartupPath + "\\temp.zip");
            // Re-enable editing
            txt_pass.Enabled = true;
            txt_userName.Enabled = true;
            loginButton.Enabled = true;
            txt_fileDir.Enabled = true;
            fileBrowseButton.Enabled = true;
            txt_month.Enabled = true;
            txt_day.Enabled = true;
            txt_year.Enabled = true;
            uploadButton.Enabled = true;
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
