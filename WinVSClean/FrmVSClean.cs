using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using vscleanlib;

namespace WinVSClean
{
    public delegate void NotificationDelegate(string msg, double offset);
    public partial class FrmVSClean : Form
    {
        public FrmVSClean()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void selectFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ZipSource(false);
        }

        private void ZipSource(bool includeVC)
        {
            fbdSourceFolder.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            DialogResult dr = fbdSourceFolder.ShowDialog();
            if (dr == DialogResult.OK)
            {
                logTxt = new StringBuilder();
                Cleaner.ProgressNotify = Notifier;
                Cleaner.SourceBackup(fbdSourceFolder.SelectedPath, !includeVC);
            }
        }

        public void Notifier(string msg, double progress)
        {
            txtLog.Invoke(new NotificationDelegate(setText), new object[] { msg, progress });
        }

        private StringBuilder logTxt;
        private void setText(string msg, double offset)
        {
            progressBar.Value = (int)(offset * 100);
            statusLabel.Text = msg;
            logTxt.AppendFormat("{0:N2}%:  {1}\r\n", offset * 100, msg);
        }

        private void viewLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtLog.Text = logTxt.ToString();
        }

        private void cleanFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fbdSourceFolder.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            DialogResult dr = fbdSourceFolder.ShowDialog();
            if (dr == DialogResult.OK)
            {
                logTxt = new StringBuilder();
                Cleaner.ProgressNotify = Notifier;
                Cleaner.SourceClean(fbdSourceFolder.SelectedPath);
            }
        }

        private void zIPSourceAndVCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ZipSource(true);
        }
    }
}
