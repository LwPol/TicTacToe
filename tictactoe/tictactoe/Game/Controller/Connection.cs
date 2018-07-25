using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Concurrent;

namespace TicTacToe.Game.Controller
{
    /// <summary>
    /// Provides mechanism for managing network TCP connection for tic-tac-toe game session.
    /// </summary>
    public class Connection : IDisposable
    {
        private TcpClient connection;
        private Thread sendingThread;
        private BlockingCollection<byte[]> dataToSend;
        private CancellationTokenSource cancelTokenSrc;

        /// <summary>
        /// Occurs when TCP connection has been lost.
        /// </summary>
        public event EventHandler ConnectionLost;

        /// <summary>
        /// Initializes a new <see cref="Connection"/> instance with specified <see cref="TcpClient"/>
        /// connection.
        /// </summary>
        /// <param name="tcpConnection"><see cref="TcpClient"/> connection to wrap.</param>
        public Connection(TcpClient tcpConnection)
        {
            connection = tcpConnection;
            DataProcessor = new ConnectionDataProcessor();

            new Thread(new ThreadStart(ReceiveData))
            {
                IsBackground = true
            }.Start();


            sendingThread = new Thread(new ThreadStart(SendingDataProc))
            {
                IsBackground = true
            };
            dataToSend = new BlockingCollection<byte[]>();
            cancelTokenSrc = new CancellationTokenSource();
            sendingThread.Start();
        }

        /// <summary>
        /// Disposes of resources used by this instance of <see cref="Connection"/>.
        /// </summary>
        public void Dispose()
        {
            cancelTokenSrc.Cancel();
            sendingThread.Join();
            connection.Dispose();
            cancelTokenSrc.Dispose();
            dataToSend.Dispose();
        }

        /// <summary>
        /// Asynchroniusly sends piece of information accross network connection.
        /// </summary>
        /// <param name="infoCode">Code of operation.</param>
        /// <param name="infoData">Additional data about the operation.</param>
        public void SendInformation(string infoCode, object infoData)
        {
            SendData(DataProcessor.Assemble(infoCode, infoData));
        }

        /// <summary>
        /// Receives data from network stream and processes it.
        /// </summary>
        private void ReceiveData()
        {
            try
            {
                NetworkStream stream = connection.GetStream();
                byte[] buffer = new byte[256];

                for (int bytesRead; (bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0;)
                {
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    DataProcessor.Process(data);
                }
            }
            catch (IOException)
            {
                ConnectionLost?.Invoke(this, EventArgs.Empty);
            }
            catch (ObjectDisposedException)
            {
                ConnectionLost?.Invoke(this, EventArgs.Empty);
            }
            catch (InvalidOperationException)
            {
                ConnectionLost?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Adds data to queue to be sent across network.
        /// </summary>
        /// <param name="data">Data to send.</param>
        private void SendData(string data)
        {
            dataToSend.Add(Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// Retrives data from queue and sends it across network.
        /// </summary>
        private void SendingDataProc()
        {
            byte[] data = null;
            try
            {
                while (true)
                {
                    data = dataToSend.Take(cancelTokenSrc.Token);
                    connection.GetStream().Write(data, 0, data.Length);
                }
            }
            catch (OperationCanceledException)
            {
                try
                {
                    if (dataToSend.TryTake(out data))
                    {
                        connection.GetStream().Write(data, 0, data.Length);
                    }
                }
                catch (IOException)
                {
                }
            }
            catch (IOException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        /// <summary>
        /// Gets <see cref="ConnectionDataProcessor"/> used by this instance.
        /// </summary>
        public ConnectionDataProcessor DataProcessor
        {
            get;
        }
    }
}
