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
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
            
        }
        static TcpListener ServerSocket = new TcpListener(IPAddress.Any, 5500);
        public static bool ServerOpen = true;
        private static void OnClientConnect(IAsyncResult result)
        {
            TcpClient NewClient = ServerSocket.EndAcceptTcpClient(result);  //Stop accepting new connections into this socket
            ServerSocket.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);    //start accpeting new connections in new callback
            ClientManager.CreateNewConnection(NewClient);   //map the new client connection into our dictionary in the client manager class
        }          
        public static void NetworkingThreadProc(Object Data)
        {
            //Perform all the servers networking tasks in this thread
            //Server.InitializeServer();

            //Database.InitializeMySQLServer();

            //Have the thread tell the main program when its finished
            NetworkingThreadEndDelegate NetworkingEndDelegate = Data as NetworkingThreadEndDelegate;
            if (NetworkingEndDelegate != null)
                NetworkingEndDelegate();
        }
        delegate void NetworkingThreadEndDelegate();
        protected void NetworkingThreadEnd()
        {
            UpdateListView("Running", "Networking thread: ", true);
        }
        public static void PhysicsThreadProc(Object Data)
        {
            Globals.game = new WorldRenderer();
            Globals.game.Run();
            PhysicsThreadEndDelegate PhysicsEndDelegate = Data as PhysicsThreadEndDelegate;
            if (PhysicsEndDelegate != null)
                PhysicsEndDelegate();
        }
        delegate void PhysicsThreadEndDelegate();
         protected void PhysicsThreadEnd()
        {
            UpdateListView("Stop", "Physics thread: ", true);
        }
        delegate void SetStatusCallback(String text);
        public void SetStatus(String text)
        {
            if (statusStrip1.InvokeRequired)
            {
                SetStatusCallback d = new SetStatusCallback(SetStatus);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                tsslConnections.Text = text;
            }
        }

        private void Server_Load(object sender, EventArgs e)
        {
            Server f1 = new Server();
            f1.Text = "Running Form";
            tsslInfo.Text = "Waiting!";
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btn_run_Click_1(object sender, EventArgs e)
        {
            MySQL.mySQLSettings.user = "root";
            MySQL.mySQLSettings.password = passwordtxt.Text;
            MySQL.mySQLSettings.server = Hosttxt.Text;
            MySQL.mySQLSettings.database = databasetxt.Text;

            if (MySQL.ConnectToMySQL() == true)
            {
                try
                {
                    PacketReader.InitializePackets();   //register packet handler functions
                    ServerSocket.Start();   //start the server tcp
                    ServerSocket.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);

                    databasetxt.Enabled = Hosttxt.Enabled = passwordtxt.Enabled  = btn_run.Enabled = Porttxt.Enabled = false;
                    Thread NetworkingThread = new Thread(new ParameterizedThreadStart(NetworkingThreadProc));
                    NetworkingThreadEndDelegate NetworkingEndDelegate = NetworkingThreadEnd;
                    NetworkingThread.Start(NetworkingEndDelegate);
                    Thread PhysicsThread = new Thread(new ParameterizedThreadStart(PhysicsThreadProc));
                    PhysicsThreadEndDelegate PhysicsEndDelegate = PhysicsThreadEnd;
                    PhysicsThread.Start(PhysicsEndDelegate);
                    PacketReader _modalInicio = new PacketReader();
                    _modalInicio.ShowDialog();
                    Console.Title = "MMO" + " -- Players online: " + 0 + " / Max online: " + 0 + "";
                    string msg = "Server:" + " Online";
                    tsslInfo.Text = "Running!";
                    SetStatus(msg);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                string msg = "MySql:" + " Failed";
                tsslInfo.Text = "Error!";
                SetStatus(msg);
            }
        }

        public void UpdateListView(string stat, string msj, bool status)
        {
            lvClients.Invoke((MethodInvoker)delegate {
                if (status == true)
                {
                ListViewItem lvi;
                lvi = new ListViewItem(stat, 0);
                var values = msj.Split(':');
                lvi.SubItems.Add(msj.Substring(0, msj.Length - values[values.Count() - 1].Length - 1));
                lvi.SubItems.Add(values[values.Count() - 1]);
                lvClients.Items.Add(lvi);
            }
            });
        }
    }
}
