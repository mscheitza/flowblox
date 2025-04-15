using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

namespace FlowBlox
{
    internal partial class SplashWindow : Form
    {
        private static readonly int _minimumLoadingTimeInMilliseconds = 2000;

        public SplashWindow()
        {
            InitializeComponent();
        }

        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var start = DateTime.Now;
            AppWindow.AppWindow.InitApp();
            var duration = DateTime.Now - start;
            if (duration.TotalMilliseconds < _minimumLoadingTimeInMilliseconds)
            {
                Thread.Sleep(_minimumLoadingTimeInMilliseconds - (int)duration.TotalMilliseconds);
            }
        }

        private void SplashWindow_Activated(object sender, EventArgs e)
        {
            backgroundWorker.RunWorkerAsync();
        }
    }
}