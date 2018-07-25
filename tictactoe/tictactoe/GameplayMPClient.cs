using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Net.Sockets;
using TicTacToe.Game.Model;
using TicTacToe.Game.Controller;

namespace TicTacToe
{
    /// <summary>
    /// Provides <see cref="AppControl"/> class for client of multiplayer gameplay.
    /// </summary>
    public class GameplayMPClient : GameplayMP
    {
        /// <summary>
        /// Initializes a new instance of <see cref="GameplayMPClient"/> with specified size of the map
        /// and <see cref="TcpClient"/> to use for communication with host.
        /// </summary>
        /// <param name="mapSize">Size of the map to play with.</param>
        /// <param name="client"><see cref="TcpClient"/> to use for communication with host.</param>
        public GameplayMPClient(Size mapSize, TcpClient client) :
            base(mapSize, Mark.Nought)
        {
            NetPlayer remote = new NetPlayer();
            connection = new Connection(client);
            
            controller = new ClientController(mapSize, connection, 10)
            {
                SyncConext = SynchronizationContext.Current
            };
            controller.AddPlayers(remote, new User());

            ClientTimer clientTimer = controller.Timing as ClientTimer;
            connection.DataProcessor.RegisterProcessingUnit(NetPlayer.MarkCode, remote);
            connection.DataProcessor.RegisterProcessingUnit(NetPlayer.QuitCode, remote);
            connection.DataProcessor.RegisterProcessingUnit(ClientTimer.TimeCode, clientTimer);
            connection.DataProcessor.RegisterProcessingUnit(ClientTimer.TimePassedCode, clientTimer);
        }
    }
}
