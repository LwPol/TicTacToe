using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Drawing;
using TicTacToe.Game.Model;

namespace TicTacToe
{
    class PrimitiveGui : AppControl
    {
        private System.Windows.Forms.Button btn1v1;
        private System.Windows.Forms.Button btnAI;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnHost;
        private System.Windows.Forms.Button btnQuit;

        public override void Dispose()
        {
            btn1v1.Dispose();
            btnAI.Dispose();
            btnConnect.Dispose();
            btnHost.Dispose();
            btnQuit.Dispose();
        }

        public override void InitializeWindow()
        {
            InitializeComponents();

            // attach event handlers
            btn1v1.Click += new EventHandler(Play1v1ButtonClick);
            btnQuit.Click += new EventHandler(QuitButtonClick);
            btnAI.Click += new EventHandler(PlayWithAIButtonClick);
            btnHost.Click += new EventHandler(HostButtonClick);
            btnConnect.Click += new EventHandler(ConnectButtonClick);
        }

        private void ConnectButtonClick(object sender, EventArgs e)
        {
            using (ClientDlg dlg = new ClientDlg())
            {
                var ret = dlg.ShowDialog();
                if (ret == DialogResult.OK)
                {
                    TcpClient client;
                    try
                    {
                        client = new TcpClient();
                        client.Connect(new IPEndPoint(IPAddress.Parse((string)dlg.Tag), 11000));
                        CtrlInstance = new GameplayMPClient(new Size(100, 100), client);
                    }
                    catch (SocketException)
                    {

                    }
                }
            }
        }

        private void Play1v1ButtonClick(object sender, EventArgs e)
        {
            CtrlInstance = new GameplaySP(new Size(100, 100));
        }

        private void PlayWithAIButtonClick(object sender, EventArgs e)
        {
            CtrlInstance = new GameplayWithStupidAI(new Size(100, 100));
        }

        private void QuitButtonClick(object sender, EventArgs e)
        {
            MainWindow.Close();
        }

        private void HostButtonClick(object sender, EventArgs e)
        {
            var ret = MessageBox.Show("Postawić na hoście lokalnym?", "...", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            IPAddress ip;
            if (ret == DialogResult.Yes)
            {
                ip = IPAddress.Parse("127.0.0.1");
            }
            else
            {
                ip = IPAddress.Any;
            }

            TcpClient client = null;
            string title = MainWindow.Text;
            MainWindow.Text = "Oczekiwanie na połączenie...";
            Thread listeningThread = null;
            TcpListener listen = null;
            listeningThread = new Thread(() =>
            {
                listen = new TcpListener(ip, 11000);
                try
                {
                    listen.Start();
                    client = listen.AcceptTcpClient();
                }
                catch (SocketException)
                {
                }
                finally
                {
                    listen.Stop();

                    MainWindow.BeginInvoke(new MethodInvoker(() =>
                    {
                        MainWindow.Text = title;
                        listeningThread.Join();

                        if (client != null)
                        {
                            CtrlInstance = new GameplayMPHost(new Size(100, 100), client);
                        }
                    }));
                }
            });

            btnHost.Click -= HostButtonClick;
            btnHost.Text = "Przerwij";
            void NewEventHandler(object sender2, EventArgs e2)
            {
                if (listen != null)
                {
                    listen.Stop();
                }

                btnHost.Text = "Postaw";
                btnHost.Click -= NewEventHandler;
                btnHost.Click += HostButtonClick;
            }
            btnHost.Click += NewEventHandler;

            listeningThread.Start();
        }

        private void InitializeComponents()
        {
            this.btn1v1 = new System.Windows.Forms.Button();
            this.btnAI = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnHost = new System.Windows.Forms.Button();
            this.btnQuit = new System.Windows.Forms.Button();
            MainWindow.SuspendLayout();

            int xPos = (MainWindow.Width - 179) / 2;

            // 
            // btn1v1
            // 
            this.btn1v1.Location = new System.Drawing.Point(xPos, 47);
            this.btn1v1.Name = "btn1v1";
            this.btn1v1.Size = new System.Drawing.Size(179, 58);
            this.btn1v1.TabIndex = 0;
            this.btn1v1.Text = "Graj 1v1";
            this.btn1v1.UseVisualStyleBackColor = true;
            // 
            // btnAI
            // 
            this.btnAI.Location = new System.Drawing.Point(xPos, 111);
            this.btnAI.Name = "btnAI";
            this.btnAI.Size = new System.Drawing.Size(179, 58);
            this.btnAI.TabIndex = 1;
            this.btnAI.Text = "Graj z CPU";
            this.btnAI.UseVisualStyleBackColor = true;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(xPos, 175);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(179, 58);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "Połącz";
            this.btnConnect.UseVisualStyleBackColor = true;
            // 
            // btnHost
            // 
            this.btnHost.Location = new System.Drawing.Point(xPos, 239);
            this.btnHost.Name = "btnHost";
            this.btnHost.Size = new System.Drawing.Size(179, 58);
            this.btnHost.TabIndex = 3;
            this.btnHost.Text = "Hostuj";
            this.btnHost.UseVisualStyleBackColor = true;
            // 
            // btnQuit
            // 
            this.btnQuit.Location = new System.Drawing.Point(xPos, 303);
            this.btnQuit.Name = "btnQuit";
            this.btnQuit.Size = new System.Drawing.Size(179, 58);
            this.btnQuit.TabIndex = 4;
            this.btnQuit.Text = "Wyjdź";
            this.btnQuit.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            MainWindow.Controls.Add(this.btnQuit);
            MainWindow.Controls.Add(this.btnHost);
            MainWindow.Controls.Add(this.btnConnect);
            MainWindow.Controls.Add(this.btnAI);
            MainWindow.Controls.Add(this.btn1v1);

            MainWindow.ResumeLayout(false);
        }
    }
}
