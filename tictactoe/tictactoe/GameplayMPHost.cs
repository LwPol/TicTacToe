using System;
using System.Threading;
using System.Drawing;
using System.Net.Sockets;
using TicTacToe.Game.Model;
using TicTacToe.Game.Controller;
using System.Windows.Forms;

namespace TicTacToe
{
    /// <summary>
    /// Provides <see cref="AppControl"/> class for host of multiplayer gameplay.
    /// </summary>
    public class GameplayMPHost : GameplayMP
    {
        /// <summary>
        /// Initializes a new instance of <see cref="GameplayMPHost"/> with specified size of map and 
        /// <see cref="TcpClient"/> to use for communication with client.
        /// </summary>
        /// <param name="mapSize">Size of the map to play with.</param>
        /// <param name="client"><see cref="TcpClient"/> to use for communication with client.</param>
        public GameplayMPHost(Size mapSize, TcpClient client) :
            base(mapSize, Mark.Cross)
        {
            NetPlayer remote = new NetPlayer();
            connection = new Connection(client);
            
            controller = new HostController(mapSize, connection, 10)
            {
                SyncConext = SynchronizationContext.Current
            };
            controller.AddPlayers(new User(), remote);

            HostTimer timing = controller.Timing as HostTimer;
            connection.DataProcessor.RegisterProcessingUnit(NetPlayer.MarkCode, remote);
            connection.DataProcessor.RegisterProcessingUnit(NetPlayer.QuitCode, remote);
            connection.DataProcessor.RegisterProcessingUnit(ClientTimer.TimeCode, timing);
            connection.DataProcessor.RegisterProcessingUnit(ClientTimer.TimePassedCode, timing);

            controller.Timing.Tick += (sender, e) =>
            {
                TimeSpan time = TimeSpan.FromSeconds(controller.Timing.TimeLeft);
                connection.SendInformation(ClientTimer.TimeCode, time.ToString());
            };
            controller.Timing.TimePassed += (sender, e) =>
            {
                connection.SendInformation(ClientTimer.TimePassedCode, null);
            };
            controller.State.CurrentChanged += (sender, e) =>
            {
                MainWindow.BeginInvoke(new MethodInvoker(() =>
                {
                    controller.Timing.Restart();
                }));
            };
        }

        /// <summary>
        /// Disposes of resources held by this instance of <see cref="GameplayMPHost"/>.
        /// </summary>
        public override void Dispose()
        {
            (controller.Timing as HostTimer).Dispose();
            base.Dispose();
        }
    }
}
