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
        int dupesCount = 0;

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

            var linesHash = new HashSet<int>();
            int lineNumber = 0;
            while (!sourceReader.EndOfStream)
            {
                string line = sourceReader.ReadLine();
                bwProcessFiles.ReportProgress((int)((lineNumber++ * 100) / numLines));
                int hash = line.GetHashCode();
                if (linesHash.Contains(hash))
                {
                    // if destinationWriter contains line string, continue
                    destinationWriter.Flush();
                    destinationWriter.Close();
                    temp = new StreamReader(destinationFilename);
                    bool found = false;
                    while (!temp.EndOfStream)
                    {
                        if (line == temp.ReadLine())
                        {
                            found = true;
                            dupesCount++;
                            break;
                        }
                    }

                    temp.Close();
                    destinationWriter = new StreamWriter(destinationFilename, true);
                    if (found)
                    {
                        continue;
                    }
                    else
                    {
                        destinationWriter.WriteLine(line);
                    }
                }
                else
                {
                    linesHash.Add(hash);
                    destinationWriter.WriteLine(line);
                }
            }
        }

        private void bwProcessFiles_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            label1.Text = e.ProgressPercentage.ToString() + "%";
        }

        private void bwProcessFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Default;
            sourceReader.Close();
            destinationWriter.Flush();
            destinationWriter.Close();
            label1.Text = dupesCount.ToString();
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
