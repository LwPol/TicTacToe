using System;
using System.Drawing;

namespace TicTacToe.Game.Model
{
    /// <summary>
    /// Represents a user of this program.
    /// </summary>
    public class User : Player
    {
        /// <summary>
        /// Occurs when it's player's turn to make a move.
        /// </summary>
        public event EventHandler UsersTurn;

        /// <summary>
        /// Informs user that it's his turn.
        /// </summary>
        public override void MakeMove()
        {
            UsersTurn?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Propagates user's request about marking a field.
        /// </summary>
        /// <param name="point">Field coordinate to mark.</param>
        public void MarkPoint(Point point)
        {
            OnMoveRequest(new PlayerMoveEventArgs(point));
        }
    }
}
