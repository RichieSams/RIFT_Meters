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

            // Create general connection. Will need to change this to prevent security loss from someone decompiling
            String genConString = "server=personaguild.com; User Id=admin; database=persona_EPGP; Password=ilike333";
            connection = new MySqlConnection(genConString);

            // Set up progress bar
            uploadProgress.Value = 0;
            uploadProgress.Maximum = File.ReadLines(logDir).Count();

            try
            {
                // Open file
                FileStream fs = new FileStream(logDir, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(fs);

                String line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    uploadProgress.PerformStep();
                }
            }
            catch (IOException)
            {
                MessageBox.Show("File does not exist or was entered incorrectly. Please enter a file or browse to it, then retry.", "Incorrect file");
                return;
            }
        }


    }
}
