using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LogParser
{
    public partial class riftParser : Form
    {
        public riftParser()
        {
            InitializeComponent();
        }

        private void fileBrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Multiselect = false;
            ofd.AddExtension = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txt_fileDir.Text = ofd.FileName;
            }

        }
    }
}
