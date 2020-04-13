using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GeoIT_Infrastructure.Log
{
    public partial class LogForm : Form
    {
        public LogForm()
        {
            InitializeComponent();
        }

        private void LogForm_Load(object sender, EventArgs e)
        {
            var filename = LogManager.GetCurrentLogFile();
            if(!File.Exists(filename))
            {
                MessageBox.Show("No log file available");
                Close();
                return;
            }

            tbLog.Text = File.ReadAllText(filename);

            tbLog.SelectionStart = tbLog.TextLength;
            tbLog.ScrollToCaret();

            LogManager.OnLog += LogManager_OnLog;
        }

        void LogManager_OnLog(string name, string logType, string format, params object[] args)
        {
            this.OnThread(() =>
            {
                tbLog.Text += string.Format("{3} - {0:dd/MM/yyyy HH:mm:ss} - [{2}] {1}", DateTime.Now, string.Format(format.TrimEnd('\n').TrimEnd('\r'), args), name, logType);

                tbLog.SelectionStart = tbLog.TextLength;
                tbLog.ScrollToCaret();
            });
        }

        private void LogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogManager.OnLog -= LogManager_OnLog;
        }
    }
}
