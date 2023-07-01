using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace Spotify_Playlist_Downloader
{
    public partial class Form1 : Form
    {
        private DownloadHelper helper;

        private Boolean downloadStarted = false;

        public Form1()
        {
            InitializeComponent();
            EnableElements();
            textBox_PlaylistID.Text = "https://open.spotify.com/playlist/3QiMdBlrkQuqI6xWSfDaBH?si=4f597c0f92f04114";
        }

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
            //helper.DownloadAll(Environment.CurrentDirectory + @"\\Downloads");
            //ShowResult();

            if (downloadStarted)
            {
                // stop work
                textBox_PlaylistID.Text = "Cancelling...";
                this.backgroundWorker1.CancelAsync();
            }
            else
            {
                // start work
                this.backgroundWorker1.RunWorkerAsync(2000);
                downloadStarted = !downloadStarted;
            }
            EnableElements();
        }

        private void TextBox_PlaylistID_TextChanged(object sender, EventArgs e)
        {
            EnableElements();
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // Do not access the form's BackgroundWorker reference directly.
            // Instead, use the reference provided by the sender parameter.
            BackgroundWorker bw = sender as BackgroundWorker;

            // Extract the argument.
            int arg = (int)e.Argument;

            // Start the time-consuming operation.
            e.Result = TimeConsumingOperation(bw, arg);

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
        private int TimeConsumingOperation(
            BackgroundWorker bw,
            int sleepPeriod)
        {
            int result = 0;

            Random rand = new Random();

            while (!bw.CancellationPending)
            {
                bool exit = false;

                switch (rand.Next(3))
                {
                    // Raise an exception.
                    case 0:
                        {
                            throw new Exception("An error condition occurred.");
                            break;
                        }

                    // Sleep for the number of milliseconds
                    // specified by the sleepPeriod parameter.
                    case 1:
                        {
                            Thread.Sleep(sleepPeriod);
                            break;
                        }

                    // Exit and return normally.
                    case 2:
                        {
                            result = 23;
                            exit = true;
                            break;
                        }

                    default:
                        {
                            break;
                        }
                }

                if (exit)
                {
                    break;
                }
            }

            return result;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // The user canceled the operation.
                MessageBox.Show("Operation was canceled");
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                string msg = String.Format("An error occurred: {0}", e.Error.Message);
                MessageBox.Show(msg);
            }
            else
            {
                // The operation completed normally.
                string msg = String.Format("Result = {0}", e.Result);
                MessageBox.Show(msg);
            }
            downloadStarted = false;
            EnableElements();
        }
    }
}
