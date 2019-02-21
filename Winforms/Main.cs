using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Swaelo_Server
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            Globals.MainWindowForm = this;
            this.Text = "MMO" + " -- Players online: " + 0 + " / Max online: " + 0 + "";
        }

        private void Main_Load(object sender, EventArgs e)
        {
            this.Text = "Mmo " + "Players Online: " + PacketReaderLogic.Count + " MaxOnline: " + PacketReaderLogic.MaxOn;
        }

        private void Server_Load(object sender, EventArgs e)
        {
            tsslInfo.Text = "Waiting!";
        }

        private void btn_run_Click(object sender, EventArgs e)
        {
            //start the database connection
            string DBUser = "root";
            string DBPass = passwordtxt.Text;
            string DBServer = Hosttxt.Text;
            string DBPort = "3306";
            Globals.database = new Database(DBUser, DBPass, DBServer, DBPort);

            //start the game server
            ServerLogic.InitializeServer();

            databasetxt.Enabled = Hosttxt.Enabled = passwordtxt.Enabled = btn_run.Enabled = Porttxt.Enabled = false;

            //Initialize and start all application threads
            Globals.NetworkingThread.InitializeThread();
            Globals.NetworkingThread.StartThread();
            Globals.PhysicsThread.InitializeThread();
            Globals.PhysicsThread.StartThread();

            this.Text = "MMO" + " -- Players online: " + 0 + " / Max online: " + 0 + "";
            string msg = "Server:" + " Online";
            tsslInfo.Text = "Running!";
            SetNetworkingStatus(true, msg);
        }

        //Updates the display of the networking thread status ribbon in the server app window
        delegate void SetNetworkingStatusCallback(bool Status, string Message);
        public void SetNetworkingStatus(bool ThreadStatus, string StatusMessage)
        {
            if(NetworkingThreadStatusStrip.InvokeRequired)
            {
                SetNetworkingStatusCallback Callback = new SetNetworkingStatusCallback(SetNetworkingStatus);
                this.Invoke(Callback, new object[] { ThreadStatus, StatusMessage });
            }
            else
            {
                NetworkingConnections.Text = StatusMessage;
            }
        }

        //Updates the display of the physics thread status ribbon in the server app window
        delegate void SetPhysicsStatusCallback(bool Status, string Message);
        public void SetPhysicsStatus(bool ThreadStatus, string StatusMessage)
        {
            if (PhysicsThreadStatusStrip.InvokeRequired)
            {
                SetPhysicsStatusCallback Callback = new SetPhysicsStatusCallback(SetPhysicsStatus);
                this.Invoke(Callback, new object[] { ThreadStatus, StatusMessage });
            }
            else
                PhysicsConnections.Text = StatusMessage;
        }

        //Previous message's are combod, and only 12 messages are displayed at once
        private string PreviousMessage = "";
        private int MessageCombo = 2;
        private int MessageCount = 0;
        private int MaxMessageCount = 12;
        //Displays the next chat message into the server message window
        delegate void SetLogMessageCallback(string Message);
        public void SetLogMessage(string LogMessage)
        {
            if(MessageWindow.InvokeRequired)
            {
                //Invoke this function on the correct thread if this is the wrong one
                SetLogMessageCallback Callback = new SetLogMessageCallback(SetLogMessage);
                this.Invoke(Callback, new object[] { LogMessage });
            }
            else
            {
                //Update previous message to display message combos
                if (PreviousMessage == LogMessage)
                {
                    MessageWindow.Items[MessageCount-1].Text = LogMessage + " x" + MessageCombo;
                    //increase the combo counter
                    MessageCombo++;
                }
                else
                {//display non repeating messages like normal, and reset the combo counter
                    MessageCombo = 2;
                    MessageCount++;
                    if(MessageCount >= MaxMessageCount)
                    {
                        //If we have gone beyond the limit, the first message must be removed
                        MessageWindow.Items.RemoveAt(0);
                        //Now we can decrement the total message counter
                        MessageCount--;
                    }
                    //Add the new message to the window
                    ListViewItem MessageItem = new ListViewItem(LogMessage, 0);
                    MessageWindow.Items.Add(MessageItem);
                }
                //remember this as the previous message
                PreviousMessage = LogMessage;
            }
        }
    }
}

