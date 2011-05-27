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

namespace LogParser
{
    public partial class riftParser : Form
    {
        // MySQL connection
        MySqlConnection connection;


        public riftParser()
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

            // Create connections. Will need to change this to prevent security loss from someone decompiling
            // However I want something more elegant that just creating more and more users for the table. That will get tedious for us to moderate.
            String genConString = "server=personaguild.com; User Id=persona_admin; database=persona_RIFT_logs; Password=ilike333";
            connection = new MySqlConnection(genConString);

            // Set up progress bar
            int lineCount = File.ReadLines(logDir).Count();
            uploadProgress.Value = 0;
            uploadProgress.Maximum = lineCount;

            try
            {
                // Open combat log file
                FileStream fs = new FileStream(logDir, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(fs);

                // Create csv file and the file writer
                TextWriter writer = new StreamWriter("temp.csv");

                String line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    // Only parse combat lines
                    if (line.IndexOf(")") > 0)
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

                        // Get time
                        Time = line.Substring(0, 8);

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
                        if (((int.Parse(CodeList[0]) >= 3) &&  (int.Parse(CodeList[0]) <= 23)) || (int.Parse(CodeList[0]) == 27) || (int.Parse(CodeList[0]) == 28))
                        {
                            // Set ID's and names
                            TypeID = CodeList[0];
                            SourceID = CodeList[1].Split('#')[2];
                            TargetID = CodeList[2].Split('#')[2];
                            SourceOwnerID = CodeList[3].Split('#')[2];
                            TargetOwnerID = CodeList[4].Split('#')[2];
                            SourceName = CodeList[5];
                            TargetName = CodeList[6];
                            Amount = CodeList[7];
                            SpellID = CodeList[8];
                            SpellName = CodeList[9];

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
                            writer.WriteLine(Time + "," + TypeID + "," + SourceID + "," + TargetID + "," + SpellID + "," + Amount + "," + Element + "," + AbsorbedValue + "," + BlockedValue + "," + OverhealValue + "," + OverkillValue);
                        }    
                    }

                    // Increment progress bar
                    uploadProgress.PerformStep();
                    
                }
                // Close the csv file
                writer.Close();
            }
            catch (IOException)
            {
                MessageBox.Show("File does not exist or was entered incorrectly. Please enter a file or browse to it, then retry.", "Incorrect file");
                return;
            }
                        
            MessageBox.Show("Done!!");

        }


    }
}
