using System;

namespace TicTacToe.Game.Model
{
    /// <summary>
    /// Represents abstract timer counting down time for making a move.
    /// </summary>
    public interface ITiming
    {
        /// <summary>
        /// Occurs when one second has passed since last timer tick.
        /// </summary>
        event EventHandler Tick;
        /// <summary>
        /// Occurs when time for making a move has passed.
        /// </summary>
        event EventHandler TimePassed;

        /// <summary>
        /// Restarts countdown.
        /// </summary>
        void Restart();
        /// <summary>
        /// Starts time countdown.
        /// </summary>
        void Start();
        /// <summary>
        /// Stops countdown.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets time left for making a move.
        /// </summary>
        int TimeLeft { get; }
        /// <summary>
        /// Gets time in which move needs to be made.
        /// </summary>
        int MoveTime { get; }
    }
}
