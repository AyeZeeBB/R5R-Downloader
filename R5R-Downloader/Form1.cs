using Microsoft.WindowsAPICodePack.Dialogs;
using R5R_Downloader.Properties;
using SuRGeoNix;
using SuRGeoNix.BitSwarmLib;
using SuRGeoNix.BitSwarmLib.BEP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace R5R_Downloader
{
    public partial class Form1 : Form
    {
        static Torrent torrent;
        static BitSwarm bitSwarm;
        static Options opt;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            panel1.Location = new Point(12, 40);
            downloadpanel.Location = new Point(12, 40);
            settingspanel.Location = new Point(12, 40);
            this.Size = new Size(597, 356);
            CenterToScreen();

            if(Settings.Default.DownloadPath != "")
            {
                if (!Directory.Exists(Settings.Default.DownloadPath + "/R5R-Downloading-Temp/"))
                {
                    if(!Directory.Exists(Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM"))
                    {
                        Directory.CreateDirectory(Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/platform");
                        StartR5RDetoursAndScripts();
                    }
                    MessageBox.Show("Can not find previously downloaded files, restarting download!");

                    Settings.Default.DownloadPath = "";
                    Settings.Default.Save();

                    if (Directory.Exists(Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Temp\BitSwarm")))
                        Directory.Delete(Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Temp\BitSwarm"), true);

                    panel1.Visible = true;
                    downloadpanel.Visible = false;
                    guna2Button2.Enabled = false;
                    guna2ImageButton1.Visible = false;
                    guna2ImageButton1.Enabled = false;
                    settingspanel.Visible = false;
                }
                else
                {
                    panel1.Visible = false;
                    downloadpanel.Visible = true;
                    guna2Button2.Enabled = true;
                    guna2ImageButton1.Visible = true;
                    guna2ImageButton1.Enabled = true;
                    settingspanel.Visible = false;
                }
            }
            else
            {
                panel1.Visible = true;
                downloadpanel.Visible = false;
                guna2Button2.Enabled = false;
                guna2ImageButton1.Visible = false;
                guna2ImageButton1.Enabled = false;
                settingspanel.Visible = false;
            }

            guna2AnimateWindow1.SetAnimateWindow(this, Guna.UI2.WinForms.Guna2AnimateWindow.AnimateWindowType.AW_BLEND);

            Refresh();
        }

        private void OnFormClosing()
        {
            torrent.SaveSession();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Settings.Default.DownloadPath != "")
            {
                if (button1.Text == "Start")
                {
                    output.Text = "";
                    guna2Button1.Enabled = false;
                    try
                    {
                        
                        opt = new Options();

                        opt.FolderComplete = @Settings.Default.DownloadPath;
                        opt.FolderIncomplete = @Settings.Default.DownloadPath + "/R5R-Downloading-Temp/";

                        opt.MaxTotalConnections = Settings.Default.MaxTotalConnections;
                        opt.MaxNewConnections = Settings.Default.MaxNewConnections;
                        opt.PeersFromTracker = Settings.Default.PeersFromTracker;
                        opt.BlockRequests = 500;
                        opt.ConnectionTimeout = Settings.Default.ConnectionTimeout;
                        opt.HandshakeTimeout = Settings.Default.HandshakeTimeout;
                        opt.PieceTimeout = Settings.Default.PieceTimeout;

                        opt.Verbosity = 0;
                        opt.LogDHT = false;
                        opt.LogStats = false;
                        opt.LogTracker = false;
                        opt.LogPeer = false;

                        output.Text = "Started at " + DateTime.Now.ToString("G", DateTimeFormatInfo.InvariantInfo) + "\r\n";
                        button1.Text = "Stop";

                        bitSwarm = new BitSwarm(opt);
                        bitSwarm.StatsUpdated += BitSwarm_StatsUpdated;
                        bitSwarm.MetadataReceived += BitSwarm_MetadataReceived;
                        bitSwarm.StatusChanged += BitSwarm_StatusChanged;

                            bitSwarm.Open("magnet:?xt=urn:btih:KCQJQT6DV2V4XWCOKCRM4EJELRLHQKI5&dn=R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM&tr=udp%3A%2F%2Fwambo.club%3A1337%2Fannounce");
                        bitSwarm.Start();
                    }
                    catch (Exception e1)
                    {
                        MessageBox.Show("Can not find previously downloaded files, restarting download!");

                        if (Directory.Exists(Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Temp\BitSwarm")))
                            Directory.Delete(Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Temp\BitSwarm"), true);

                        button1.Text = "Start";
                        button1.PerformClick();
                    }
                }
                else
                {
                    bitSwarm.Dispose();
                    button1.Text = "Start";
                    guna2Button1.Enabled = true;
                }
            } 
            else
            {
                MessageBox.Show("Please select a path to download to!");
            }
        }

        private void BitSwarm_MetadataReceived(object source, BitSwarm.MetadataReceivedArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => BitSwarm_MetadataReceived(source, e)));
                return;
            }
            else
            {
                torrent = e.Torrent;
                output.Text += bitSwarm.DumpTorrent().Replace("\n", "\r\n");
            }
        }
        private void BitSwarm_StatusChanged(object source, BitSwarm.StatusChangedArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => BitSwarm_StatusChanged(this, e)));
                return;
            }

            button1.Text = "Start";

            if (e.Status == 0)
            {
                string fileName = "";
                if (torrent.file.name != null) fileName = torrent.file.name;
                if (torrent != null) { torrent.Dispose(); torrent = null; }

                output.Text += "\r\n\r\nFinished at " + DateTime.Now.ToString("G", DateTimeFormatInfo.InvariantInfo);
                MessageBox.Show("Downloaded successfully!\r\n"/* + "Starting detours and scripts install."*/);
                //StartR5RDetoursAndScripts();
            }
            else
            {
                output.Text += "\r\n\r\nStopped at " + DateTime.Now.ToString("G", DateTimeFormatInfo.InvariantInfo);

                if (e.Status == 2)
                {
                    output.Text += "\r\n\r\n" + "An error occurred :(\r\n\t" + e.ErrorMsg;
                    MessageBox.Show("An error occured :( \r\n" + e.ErrorMsg);
                }
            }

            if (torrent != null) torrent.Dispose();
        }
        private void BitSwarm_StatsUpdated(object source, BitSwarm.StatsUpdatedArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => BitSwarm_StatsUpdated(source, e)));
                return;
            }
            else
            {
                downRate.Text = String.Format("{0:n0}", (e.Stats.DownRate / 1024)) + " KB/s";
                downRateAvg.Text = String.Format("{0:n0}", (e.Stats.AvgRate / 1024)) + " KB/s";
                eta.Text = TimeSpan.FromSeconds((e.Stats.ETA + e.Stats.AvgETA) / 2).ToString(@"hh\:mm\:ss");
                bDownloaded.Text = Utils.BytesToReadableString(e.Stats.BytesDownloaded + e.Stats.BytesDownloadedPrevSession);
                dpeers.Text = e.Stats.PeersTotal.ToString();

                if (torrent != null && torrent.data.totalSize != 0)
                {
                    Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance.SetProgressValue(e.Stats.Progress, 100);
                    progress.Value = e.Stats.Progress;
                }
            }

        }


        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        WebClient scriptsdownload = new WebClient();
        private void StartR5RDetours()
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                bDownloaded.Text = "Downloading Detours";
            });

            string randomestring = RandomString(10);


            string downloadString = scriptsdownload.DownloadString("https://r5reloaded.com/api/v1.php?data=detours");
            scriptsdownload.DownloadFile(new Uri(downloadString), @Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/detours-" + randomestring + ".zip");

            Thread.Sleep(1000);

            this.BeginInvoke((MethodInvoker)delegate
            {
                bDownloaded.Text = "Installing Detours";
            });

            var detoursextract = ZipFile.Open(Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/detours-" + randomestring + ".zip", ZipArchiveMode.Read);
            ZipArchiveExtensions.ExtractToDirectory(detoursextract, Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/", true);
            detoursextract.Dispose();

            File.Delete(Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/detours-" + randomestring + ".zip");


            if (!Directory.Exists(Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/platform"))
            {
                Directory.CreateDirectory(Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/platform");
            }

            this.BeginInvoke((MethodInvoker)delegate
            {
                bDownloaded.Text = "Downloading Scripts";
            });

        }
        #region DetoursAndScripts

        private void StartR5RScripts()
        {
            scriptsdownload.DownloadFile(new Uri("https://github.com/Mauler125/scripts_r5/archive/refs/heads/S3_N1094.zip"), @Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/platform/newscripts.zip");


            if (Directory.Exists(Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/platform/scripts"))
            {
                Directory.Delete(Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/platform/scripts", true);
            }

            var scriptszip = ZipFile.Open(Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/platform/newscripts.zip", ZipArchiveMode.Read);
            ZipArchiveExtensions.ExtractToDirectory(scriptszip, Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/platform/", true);
            scriptszip.Dispose();

            this.BeginInvoke((MethodInvoker)delegate
            {
                bDownloaded.Text = "Installing Scripts";
            });


            File.Delete(Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/platform/newscripts.zip");

            Directory.Move(Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/platform/scripts_r5-S3_N1094", Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/platform/scripts");

            this.BeginInvoke((MethodInvoker)delegate
            {
                MessageBox.Show("Scripts and detours have been installed!");
                bDownloaded.Text = "Installing Complete";
            });
        }

        private void StartR5RDetoursAndScripts()
        {
            if (Directory.Exists(Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/"))
            {
                StartR5RDetours();
                StartR5RScripts();
                    
            }
            else
            {
                MessageBox.Show("Somthing went wrong and cant continue!");
            }
        }

        #endregion
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bitSwarm != null) bitSwarm.Dispose();
        } 

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "Select a folder to download to";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Settings.Default.DownloadPath = dialog.FileName;
                Settings.Default.Save();
                guna2TextBox1.Text = Settings.Default.DownloadPath;
                UpdateContinue();
            }
        }
        
        private void UpdateContinue()
        {
            if (Settings.Default.DownloadPath != "")
            {
                guna2Button2.Enabled = true;
            }
            else
            {
                guna2Button2.Enabled = false;
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if(Settings.Default.DownloadPath != "")
            {
                guna2Transition1.Hide(panel1);
                guna2Transition1.Show(downloadpanel);

                guna2ImageButton1.Visible = true;
                guna2ImageButton1.Enabled = true;
            }
            else
            {
                MessageBox.Show("Please select a path to download to!");
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            Settings.Default.MaxTotalConnections = ((int)totalcons.Value);
            Settings.Default.MaxNewConnections = ((int)newcons.Value);
            Settings.Default.PeersFromTracker = ((int)pft.Value);
            Settings.Default.ConnectionTimeout = ((int)contimeout.Value);
            Settings.Default.HandshakeTimeout = ((int)handtimeout.Value);
            Settings.Default.PieceTimeout = ((int)peicetimeout.Value);
            Settings.Default.MetadataTimeout = ((int)metatimeout.Value);
            Settings.Default.Save();

            guna2Transition1.Hide(settingspanel);
            guna2Transition1.Show(downloadpanel);
        }

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            totalcons.Value = Settings.Default.MaxTotalConnections;
            newcons.Value = Settings.Default.MaxNewConnections;
            pft.Value = Settings.Default.PeersFromTracker;
            contimeout.Value = Settings.Default.ConnectionTimeout;
            handtimeout.Value = Settings.Default.HandshakeTimeout;
            peicetimeout.Value = Settings.Default.PieceTimeout;
            metatimeout.Value = Settings.Default.MetadataTimeout;

            guna2Transition1.Hide(downloadpanel);
            guna2Transition1.Show(settingspanel);
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            guna2Transition1.Hide(settingspanel);
            guna2Transition1.Show(downloadpanel);
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM"))
                StartR5RDetours();
            else
                MessageBox.Show("'R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM' folder not found! Wait for the torrent to download it.");
        }


        private void guna2Button5_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Settings.Default.DownloadPath + "/R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM/platform"))
                StartR5RScripts();
            else
                MessageBox.Show("No 'platform' folder in R5pc_r5launch_N1094_CL456479_2019_10_30_05_20_PM folder");
        }
    }

    public static class ZipArchiveExtensions
    {
        public static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                string directory = Path.GetDirectoryName(completeFileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (file.Name != "")
                    file.ExtractToFile(completeFileName, true);
            }
        }
    }
}
