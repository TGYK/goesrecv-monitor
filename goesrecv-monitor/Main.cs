﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace goesrecv_monitor
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();

            // Set version label
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            labelVersion.Text = "v" + fvi.FileVersion;

            // Validate saved IP
            string IP = Properties.Settings.Default.IP;
            IPAddress temp;
            if (IPAddress.TryParse(IP, out temp))
            {
                textIP.Text = IP;
            }
            else
            {
                textIP.Text = "192.168.";
            }

            // Set control colours
            btnConnct.BackColor = Color.FromArgb(255, 30, 30, 30);
            textIP.BackColor = Color.FromArgb(255, 30, 30, 30);
            constellationPanel.BackColor = Color.FromArgb(255, 20, 20, 20);
            labelVersion.BackColor = Color.FromArgb(255, 20, 20, 20);
            labelSite.BackColor = Color.FromArgb(255, 20, 20, 20);
        }

        /// <summary>
        /// Draws symbols on constellation plot
        /// </summary>
        /// <param name="points">List of Point objects representing symbols</param>
        public void DrawSymbols(List<Point> points)
        {
            if (constellationPanel.InvokeRequired)
            {
                constellationPanel.Invoke((MethodInvoker)(() => {
                    constellationPanel.DrawSymbols(points);
                }));
            }
            else
            {
                constellationPanel.DrawSymbols(points);
            }
        }

        /// <summary>
        /// Connect button click
        /// </summary>
        private void btnConnct_Click(object sender, EventArgs e)
        {
            if (Stats.Running)
            {
                // Stop threads
                Stats.Stop();
                Symbols.Stop();

                ResetUI();
            }
            else
            {
                // Validate IP address
                IPAddress temp;
                if (!IPAddress.TryParse(textIP.Text, out temp))
                {
                    MessageBox.Show(this, "Invalid IP address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                // Save IP address
                Properties.Settings.Default.IP = textIP.Text;
                Properties.Settings.Default.Save();

                // Set IPs
                Stats.IP = textIP.Text;
                Symbols.IP = textIP.Text;

                // Start threads
                Stats.Start();
                Symbols.Start();

                // Update UI
                textIP.Enabled = false;
                btnConnct.Text = "STOP";
                btnConnct.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Trigger connect on Enter key in IP text box
        /// </summary>
        private void textIP_KeyPress(object sender, KeyPressEventArgs e)
        {
            // If key is Enter
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;  // Stop Enter char reaching text box
                btnConnct_Click(btnConnct, null);
            }
        }

        /// <summary>
        /// Resets UI elements after disconnect
        /// </summary>
        public void ResetUI()
        {
            if (textIP.InvokeRequired)
            {
                textIP.Invoke((MethodInvoker)(() => {
                    textIP.Enabled = true;
                    textIP.Focus();
                    btnConnct.Text = "Connect";
                    btnConnct.ForeColor = Color.White;
                    labelSignalLock.Text = "-";
                    labelSignalLock.BackColor = Color.Black;
                    labelSignalLock.Padding = new Padding(0, 5, 0, 0);
                    labelFreqOffset.Text = "-";
                    progressSignalQ.Value = 0;
                    labelVitErr.Text = "-";
                    labelRsErr.Text = "-";
                }));
            }
            else
            {
                textIP.Enabled = true;
                textIP.Focus();
                btnConnct.Text = "Connect";
                btnConnct.ForeColor = Color.White;
                labelSignalLock.Text = "-";
                labelSignalLock.BackColor = Color.Black;
                labelSignalLock.Padding = new Padding(0, 5, 0, 0);
                labelFreqOffset.Text = "-";
                progressSignalQ.Value = 0;
                labelVitErr.Text = "-";
                labelRsErr.Text = "-";
            }
        }

        /// <summary>
        /// Triggers graceful exit
        /// </summary>
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.GracefulExit();
        }

        /// <summary>
        /// Open site on link label click
        /// </summary>
        private void labelSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://vksdr.com/goesrecv-monitor");
        }

        /// <summary>
        /// Open GitHub repo on link label click
        /// </summary>
        private void labelVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/sam210723/goesrecv-monitor/releases/latest");
        }


        // Properties
        public bool SignalLock
        {
            set
            {
                if (value)
                {
                    if (labelSignalLock.InvokeRequired)
                    {
                        labelSignalLock.Invoke((MethodInvoker)(() =>
                        {
                            labelSignalLock.Text = "LOCKED";
                            labelSignalLock.BackColor = Color.Green;
                            labelSignalLock.Padding = new Padding(43, 5, 43, 5);
                        }));
                    }
                    else
                    {
                        labelSignalLock.Text = "LOCKED";
                        labelSignalLock.BackColor = Color.LimeGreen;
                        labelSignalLock.Padding = new Padding(43, 5, 43, 5);
                    }
                }
                else
                {
                    if (labelSignalLock.InvokeRequired)
                    {
                        labelSignalLock.Invoke((MethodInvoker)(() =>
                        {
                            labelSignalLock.Text = "UNLOCKED";
                            labelSignalLock.BackColor = Color.Red;
                            labelSignalLock.Padding = new Padding(34, 5, 34, 5);
                        }));
                    }
                    else
                    {
                        labelSignalLock.Text = "UNLOCKED";
                        labelSignalLock.BackColor = Color.Red;
                        labelSignalLock.Padding = new Padding(0, 5, 0, 5);
                    }
                }
            }
        }

        public string FrequencyOffset
        {
            get
            {
                return labelFreqOffset.Text;
            }

            set
            {
                if (labelFreqOffset.InvokeRequired)
                {
                    labelFreqOffset.Invoke((MethodInvoker)(() =>
                    {
                        labelFreqOffset.Text = value;
                    }));
                }
                else
                {
                    labelFreqOffset.Text = value;
                }
                
            }
        }

        public int SignalQuality
        {
            get
            {
                return progressSignalQ.Value;
            }

            set
            {
                if (progressSignalQ.InvokeRequired)
                {
                    progressSignalQ.Invoke((MethodInvoker)(() =>
                    {
                        progressSignalQ.Value = value;
                    }));
                }
                else
                {
                    progressSignalQ.Value = value;
                }

            }
        }

        public int ViterbiErrors
        {
            get
            {
                return int.Parse(labelVitErr.Text);
            }

            set
            {
                if (labelVitErr.InvokeRequired)
                {
                    labelVitErr.Invoke((MethodInvoker)(() =>
                    {
                        labelVitErr.Text = value.ToString();
                    }));
                }
                else
                {
                    labelVitErr.Text = value.ToString();
                }

            }
        }

        public int RSErrors
        {
            get
            {
                return int.Parse(labelVitErr.Text);
            }

            set
            {
                if (labelRsErr.InvokeRequired)
                {
                    labelRsErr.Invoke((MethodInvoker)(() =>
                    {
                        labelRsErr.Text = value.ToString();
                    }));
                }
                else
                {
                    labelRsErr.Text = value.ToString();
                }

            }
        }
    }
}
