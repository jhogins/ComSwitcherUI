using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ComSwitcherUI
{
    public partial class MainForm : Form
    {
        SwitcherPort switcherPort = new SwitcherPort();
        enum LoadError
        {
            OK,
            ConfigSchema,
            FailedToOpen
        }

        LoadError loadError = LoadError.OK;

        string lastMessage = "";
        int numChannels = 6;
        public MainForm()
        {
            InitializeComponent();

            try
            {
                switcherPort.Open();
                switcherPort.OnMessageReceived += SwitcherPort_OnMessageReceived;
            }
            catch (Exception e)
            {
                loadError = LoadError.FailedToOpen;
            }
            UpdateStatus();

            InitButtons();
        }

        private void InitButtons()
        {
            string[] configLines = File.ReadAllLines("config.csv");
            buttonTable.ColumnCount = configLines.Length;
            buttonTable.ColumnStyles.Clear();
            int iColumn = 0;
            foreach (string configLine in configLines)
            {
                buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                string[] config = configLine.Split(',');
                if (config.Length != 2)
                {
                    loadError = LoadError.ConfigSchema;
                    return;
                }
                int iChannel;
                if (!int.TryParse(config[0], out iChannel))
                {
                    loadError = LoadError.ConfigSchema;
                    return;
                }
                var button = new Button()
                {
                    Text = config[1].Trim(),
                    Dock = DockStyle.Fill,
                };
                button.Click += (obj, ea) => switcherPort.SwitchToChannel(iChannel);
                buttonTable.Controls.Add(button);
                buttonTable.SetColumn(button, iColumn);
                iColumn++;
            }
        }

        private void SwitcherPort_OnMessageReceived(string message)
        {
            lastMessage = message;
            this.Invoke(new Action(UpdateStatus));
        }

        void UpdateStatus()
        {
            string stStatus;
            if (loadError == LoadError.OK)
            {
                string connectionStatus;
                if (switcherPort.IsConnected)
                    connectionStatus = "Connected";
                else
                    connectionStatus = "Not Connected";

                stStatus = String.Format("status: {0}  last message: {1}",
                    connectionStatus,
                    lastMessage);
            }
            else if (loadError == LoadError.FailedToOpen)
            {
                stStatus = "Failed to connect. Check to see if other switcher apps are running.";
            }
            else if (loadError == LoadError.ConfigSchema)
            {
                stStatus = "Failed to load config.csv. Check to make sure there are no errors";
            }
            else
            {
                stStatus = "unknown error";
            }
            this.statusLabel.Text = stStatus;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            switcherPort.SwitchToChannel(1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switcherPort.SwitchToChannel(2);
        }
        
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            switcherPort.Close();
        }
    }
}
