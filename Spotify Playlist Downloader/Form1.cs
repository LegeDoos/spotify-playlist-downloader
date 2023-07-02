using Spotify_Playlist_Downloader.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Spotify_Playlist_Downloader
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// The download helper containing the items and code to actually download
        /// </summary>
        private DownloadHelper helper;
        /// <summary>
        /// Represents the download status
        /// </summary>
        private Boolean downloadStarted = false;
        /// <summary>
        /// Background worker pool
        /// </summary>
        private const int nWorkers = 20;
        private BackgroundWorker[] backgroundWorkers = new BackgroundWorker[nWorkers];


        public Form1()
        {
            InitializeComponent();
            EnableElements();
            textBox_PlaylistID.Text = "https://open.spotify.com/playlist/37i9dQZF1DX9u7XXOp0l5L";

            // create workers (downloaders)
            for (int i = 0; i < nWorkers; i++)
            {
                backgroundWorkers[i] = CreateWorker();
            }
        }

        #region UI related

        /// <summary>
        /// Handle form elements enabled
        /// </summary>
        private void EnableElements()
        {
            buttonGetSongs.Enabled = !string.IsNullOrEmpty(textBox_PlaylistID.Text) && !downloadStarted;
            textBox_PlaylistID.Enabled = !downloadStarted;
            btnDownloadAll.Enabled = helper != null;
            btnDownloadAll.Text = downloadStarted ? "Stop download" : "Download all";
        }

        /// <summary>
        /// Show a messagebox with the result
        /// </summary>
        private void ShowResult()
        {
            MessageBox.Show($"Done! Downloaded {helper.Downloaded} songs, skipped {helper.Skipped} already existing songs!");
        }

        private void buttonGetSongs_Click(object sender, EventArgs e)
        {
            // init
            listView_SongsList.Items.Clear();
            helper = new DownloadHelper(textBox_PlaylistID.Text);

            try
            {
                helper.GetPlayListItems();

                // playlist items to listview
                for (int i = 0; i < helper.PlayListItems.Count; i++)
                {
                    listView_SongsList.Items.Add(helper.PlayListItems[i].track.name, i);
                }
                listView_SongsList.LargeImageList = helper.PlayListItemsImageList;
            }
            catch
            {
                MessageBox.Show("Make sure sou have passed a valid playlist ID or valid URL", "Playlist Not Found ERROR");
            }

            EnableElements();
        }

        private void listView_SongsList_MouseClick(object sender, MouseEventArgs e)
        {
            helper.DownloadSingleItem(helper.PlayListItems[listView_SongsList.FocusedItem.Index], Environment.CurrentDirectory + @"\\Downloads");
            if (helper.PlayListItems[listView_SongsList.FocusedItem.Index].DownloadStatus == Models.DownloadStatus.Downloaded)
            {
                MessageBox.Show($"Done! Downloaded 1 song.");
            }
            else
            {
                MessageBox.Show($"Done! skipped 1 song.");
            }
        }

        private void paypalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/henry-richard7");
        }

        private void paypalToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/paypalme/henryrics");
        }

        private void youtubeChannelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.youtube.com/channel/UCVGasc5jr45eZUpZNHvbtWQ");
        }

        private void telegramChannelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://t.me/cracked4free");
        }

        /// <summary>
        /// Download all songs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDownloadAll_Click(object sender, EventArgs e)
        {
            if (downloadStarted)
            {
                // stop work
                btnDownloadAll.Text = "Cancelling...";
                foreach (var worker in backgroundWorkers)
                {
                    worker.CancelAsync();
                }
            }
            else
            {
                // start work
                DistributeWork();
            }
            EnableElements();
        }

        private void TextBox_PlaylistID_TextChanged(object sender, EventArgs e)
        {
            EnableElements();
        }

        #endregion

        #region Threading related
        /// <summary>
        /// Create a worker (downloader)
        /// </summary>
        /// <returns>The worker</returns>
        private BackgroundWorker CreateWorker()
        {
            var result = new System.ComponentModel.BackgroundWorker();
            result.WorkerSupportsCancellation = true;
            result.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker_DoWork);
            result.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker_RunWorkerCompleted);
            return result;
        }

        /// <summary>
        /// Start downloading if there is work available and there are free runners
        /// </summary>
        private void DistributeWork()
        {
            int nDownlaodsAvailable = helper.PlayListItems.Count(i => i.DownloadStatus == DownloadStatus.Unknown);
            int nRunnersAvailable = backgroundWorkers.Count(w => !w.IsBusy);

            // set downloadstatus
            downloadStarted = nDownlaodsAvailable > 0;
            if (!downloadStarted)
            {
                ShowResult();
            }

            // start work
            for (int j = 0; j < Math.Min(nDownlaodsAvailable, nRunnersAvailable); j++)
            {
                Item nextItem = helper.PlayListItems.First(i => i.DownloadStatus == DownloadStatus.Unknown);
                nextItem.DownloadStatus = DownloadStatus.Started;
                BackgroundWorker nextWorker = backgroundWorkers.First(w => !w.IsBusy);
                nextWorker.RunWorkerAsync(nextItem);
            }

            EnableElements();
        }

        /// <summary>
        /// Start the work for the background worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // Do not access the form's BackgroundWorker reference directly.
            // Instead, use the reference provided by the sender parameter.
            BackgroundWorker bw = sender as BackgroundWorker;

            // Extract the argument.
            Item arg = (Item)e.Argument;

            // Start the time-consuming operation.
            e.Result = DownloadItem(bw, arg);

            // If the operation was canceled by the user,
            // set the DoWorkEventArgs.Cancel property to true.
            if (bw.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        // This method models an operation that may take a long time
        // to run. It can be cancelled, it can raise an exception,
        // or it can exit normally and return a result. These outcomes
        // are chosen randomly.
        private int DownloadItem(BackgroundWorker bw, Item item)
        {
            int result = 0;

            while (!bw.CancellationPending)
            {
                helper.DownloadSingleItem(item, Environment.CurrentDirectory + @"\\Downloads");
            }

            return result;
        }

        // <summary>
        /// Code to run when a downloader completes a task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // The user canceled the operation.
                MessageBox.Show("Operation was canceled");
                downloadStarted = false;
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                string msg = String.Format("An error occurred: {0}", e.Error.Message);
                MessageBox.Show(msg);
                downloadStarted = false;
            }
            else
            {
                // The operation completed normally. Look for other items to download
                DistributeWork();
            }
            EnableElements();
        }

    #endregion

}

}
