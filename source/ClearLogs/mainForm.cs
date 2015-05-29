using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClearLogs
{
    public partial class mainForm : Form
    {
        public mainForm()
        {
            InitializeComponent();

            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
        }

        private void clearlogs_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private String _Start_Process(String executable, String args)
        {
            try
            {
                Process p = new Process();
                ProcessStartInfo psi = new ProcessStartInfo(executable);
                psi.Arguments = args;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                p.StartInfo = psi;
                p.Start();
                StreamReader processOutput = p.StandardOutput;
                String output = processOutput.ReadToEnd();
                p.WaitForExit();
                return output;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.WorkerSupportsCancellation == true)
            {
                backgroundWorker1.CancelAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker worker = sender as BackgroundWorker;

                List<String> eventLogs = _Start_Process("wevtutil.exe", "el")
                                        .Split(new string[] { "\r\n" },
                                            StringSplitOptions.RemoveEmptyEntries)
                                        .ToList();

                int i = 1;
                foreach (String item in eventLogs)
                {

                    if (worker.CancellationPending == true)
                    {
                        e.Cancel = true;
                        break;
                    }
                    else
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            report.AppendText("Cleaning Log " + item + "\r\n");
                            report.AppendText(_Start_Process("wevtutil.exe", "cl " + item) + "..\r\n");
                        });

                        System.Threading.Thread.Sleep(50);
                        worker.ReportProgress((100 * i++) / eventLogs.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            reportLabel.Text = (e.ProgressPercentage.ToString() + "%");
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                reportLabel.Text = "Canceled!";
            }
            else if (e.Error != null)
            {
                reportLabel.Text = "Error: " + e.Error.Message;
            }
            else
            {
                reportLabel.Text = "Done!";
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
