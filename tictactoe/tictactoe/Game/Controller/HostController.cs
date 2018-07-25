using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using TicTacToe.Game.Model;

namespace TicTacToe.Game.Controller
{
    /// <summary>
    /// Represents controller handling game logic at hostside in online gameplay.
    /// </summary>
    public class HostController : GameController, INetworkDataProcessor
    {
        private Connection connection;

        /// <summary>
        /// Initializes a new instance of <see cref="HostController"/> with specified map size
        /// and connection instance.
        /// </summary>
        /// <param name="mapSize">Size of the map.</param>
        /// <param name="connection">Connection with other player.</param>
        public HostController(Size mapSize, Connection connection) :
            base(mapSize)
        {
            Connection = connection;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="HostController"/> with specified map size,
        /// connection instance and time for player's move.
        /// </summary>
        /// <param name="mapSize">Size of the map.</param>
        /// <param name="connection">Connection with other player.</param>
        /// <param name="timeForMove">Time for a player to move.</param>
        public HostController(Size mapSize, Connection connection, int timeForMove) :
            base(mapSize, new HostTimer(timeForMove))
        {
            Connection = connection;
        }

        /// <summary>
        /// Handles <see cref="Player.MoveRequest"/> event
        /// (overrides <see cref="GameController.HandlePlayersMoveRequest(object, PlayerMoveEventArgs)"/>).
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">A <see cref="PlayerMoveEventArgs"/> containing event data.</param>
        public override void HandlePlayersMoveRequest(object sender, PlayerMoveEventArgs e)
        {
            Mark markOfPlayer = State.GetMarkOfPlayer(sender as Player);
            if (MarkPointIfPossible(e.MyMove, markOfPlayer))
            {
                if (sender is NetPlayer)
                {
                    Connection.SendInformation(MarkMadeAckCode, new MarkAckInfo(e.MyMove, markOfPlayer));
                }
            }
        }

        /// <summary>
        /// Implements <see cref="INetworkDataProcessor.Assemble(string, object)"/>.
        /// </summary>
        /// <param name="code">Operation code.</param>
        /// <param name="data">Additional information about operation.</param>
        /// <returns>Text representation of data to be sent.<returns>
        string INetworkDataProcessor.Assemble(string code, object data)
        {
            if (code == MarkMadeAckCode)
            {
                return code + "\n" + (MarkAckInfo)data;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Implements <see cref="INetworkDataProcessor.Process(string, string)"/>
        /// </summary>
        /// <param name="code">Operation code.</param>
        /// <param name="body">Additional information about operation.</param>
        void INetworkDataProcessor.Process(string code, string body)
        {
        }

        /// <summary>
        /// Gets or sets Connection instance used by this object.
        /// </summary>
        public Connection Connection
        {
            get
            {
                return connection;
            }

            set
            {
                connection = value;
                connection.DataProcessor.RegisterProcessingUnit(MarkMadeAckCode, this);
            }
        }

        /// <summary>
        /// Code for mark acknowledgement operation.
        /// </summary>
        public static readonly string MarkMadeAckCode = "mark_made_ack";
    }
}
