using System;
using System.Drawing;

namespace TicTacToe.Game.Model
{
    /// <summary>
    /// Represents player of tic-tac-toe game.
    /// </summary>
    public abstract class Player
    {
        /// <summary>
        /// When overriden in derived class forces a player to make a move.
        /// </summary>
        public abstract void MakeMove();

        /// <summary>
        /// Occurs when player wants to make a move.
        /// </summary>
        public event PlayerMoveEventHandler MoveRequest;

        /// <summary>
        /// Raises the MoveRequest event.
        /// </summary>
        /// <param name="e">A <see cref="PlayerMoveEventArgs"/> containg data event.</param>
        protected virtual void OnMoveRequest(PlayerMoveEventArgs e)
        {
            MoveRequest?.Invoke(this, e);
        }
    }

    /// <summary>
    /// Provides data for <see cref="Player.MoveRequest"/> event.
    /// </summary>
    public class PlayerMoveEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PlayerMoveEventArgs"/>.
        /// </summary>
        /// <param name="move">Coordinate of a field that player pointed to mark.</param>
        public PlayerMoveEventArgs(Point move)
        {
            MyMove = move;
        }

        /// <summary>
        /// Gets coordinate of a field that player pointed to mark.
        /// </summary>
        public Point MyMove
        {
            get;
        }
    }

    /// <summary>
    /// Represents method for handling <see cref="Player.MoveRequest"/> event.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">A <see cref="PlayerMoveEventArgs"/> containing event data.</param>
    public delegate void PlayerMoveEventHandler(object sender, PlayerMoveEventArgs e);

}
