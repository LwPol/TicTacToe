using System;
using System.Drawing;
using TicTacToe.Game.Controller;
using System.Threading;

namespace TicTacToe.Game.Model
{
    /// <summary>
    /// Represents remote player.
    /// </summary>
    public class NetPlayer : Player, INetworkDataProcessor
    {
        /// <summary>
        /// Occurs when player quits the game.
        /// </summary>
        public event EventHandler GameQuitted;

        /// <summary>
        /// Forces the player to make a move (overrides <see cref="Player.MakeMove"/>)
        /// </summary>
        public override void MakeMove()
        {
        }

        /// <summary>
        /// Implements <see cref="INetworkDataProcessor.Assemble(string, object)"/>.
        /// </summary>
        /// <param name="code">Operation code.</param>
        /// <param name="data">Additional information about operation.</param>
        /// <returns>Text representation of data to be sent.</returns>
        string INetworkDataProcessor.Assemble(string code, object data)
        {
            if (code == MarkCode)
            {
                var args = (MarksChangedEventArgs)data;
                string ret = code + "\n" + args.ChangedPoint.X + "," + args.ChangedPoint.Y;
                return ret;
            }
            else if (code == QuitCode)
            {
                return code;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Implements <see cref="INetworkDataProcessor.Process(string, string)"/>.
        /// </summary>
        /// <param name="code">Operation code.</param>
        /// <param name="body">Additional information about operation.</param>
        void INetworkDataProcessor.Process(string code, string body)
        {
            if (code == MarkCode)
            {
                int commaIndex = body.IndexOf(',');
                if (commaIndex != -1)
                {
                    Point point;
                    try
                    {
                        point = new Point(int.Parse(body.Substring(0, commaIndex)),
                            int.Parse(body.Substring(commaIndex + 1)));
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    
                    OnMoveRequest(new PlayerMoveEventArgs(point));
                }
            }
            else if (code == QuitCode)
            {
                GameQuitted?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Code for mark operation.
        /// </summary>
        public static readonly string MarkCode = "mark";

        /// <summary>
        /// Code for quit operation.
        /// </summary>
        public static readonly string QuitCode = "quit";
    }
}
