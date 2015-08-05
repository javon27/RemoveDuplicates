using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoveDupes
{
    public partial class Form1 : Form
    {
        string sourceFilename;
        string destinationFilename;
        StreamReader sourceReader;
        StreamWriter destinationWriter;
        int dupesCount;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void bwProcessFiles_DoWork(object sender, DoWorkEventArgs e)
        {
            var temp = new StreamReader(sourceFilename);
            int numLines = 0;
            while (!temp.EndOfStream)
            {
                temp.ReadLine();
                numLines++;
            }
            temp.Close();

            sourceReader = new StreamReader(sourceFilename);
            destinationWriter = new StreamWriter(destinationFilename);
            
            var linesString = new HashSet<string>();
            int lineNumber = 0;
            while (!sourceReader.EndOfStream)
            {
                string line = sourceReader.ReadLine();
                if (!linesString.Contains(line))
                {
                    linesString.Add(line);
                    destinationWriter.WriteLine(line);
                }
                else
                {
                    dupesCount++;
                }
                if (++lineNumber % 100 == 0)
                {
                    string[] workerResult = new string[1];
                    workerResult[0] = line;
                    bwProcessFiles.ReportProgress((int)((lineNumber * 100) / numLines), workerResult);
                }
            }
            bwProcessFiles.ReportProgress(100, new string[1] { "" });
        }

        private void bwProcessFiles_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string[] results = (string[])e.UserState;
            progressBar1.Value = e.ProgressPercentage;
            label1.Text = e.ProgressPercentage.ToString() + "%";
            lineBox.Text = results[0];
        }

        private void bwProcessFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Default;
            sourceReader.Close();
            destinationWriter.Flush();
            destinationWriter.Close();
            lineBox.Text = dupesCount.ToString() + " duplicates found.";
            button1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    sourceFilename = openFileDialog1.FileName;
                    destinationFilename = saveFileDialog1.FileName;
                    dupesCount = 0;
                    button1.Enabled = false;
                    bwProcessFiles.RunWorkerAsync();
                }
            }
        }


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
            sourceReader.Dispose();
            destinationWriter.Dispose();
            base.Dispose(disposing);
        }
    }
}
