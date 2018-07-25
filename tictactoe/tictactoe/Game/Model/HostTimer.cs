using System;
using System.Windows.Forms;
using TicTacToe.Game.Controller;

namespace TicTacToe.Game.Model
{
    /// <summary>
    /// Represents timer counting down time to move at serverside. Timing events are raised based on internal timer.
    /// </summary>
    public class HostTimer : ITiming, IDisposable, INetworkDataProcessor
    {
        private Timer timer;
        private int timeRemaining;

        /// <summary>
        /// Occurs when one second has passed since last timer tick.
        /// </summary>
        public event EventHandler Tick;
        /// <summary>
        /// Occurs when time for making a move has passed.
        /// </summary>
        public event EventHandler TimePassed;

        /// <summary>
        /// Initializes a new instance of <see cref="HostTimer"/> with specified time for making a move.
        /// </summary>
        /// <param name="moveTime">Time for a player to make a move.</param>
        public HostTimer(int moveTime)
        {
            MoveTime = moveTime;
            timeRemaining = moveTime;
            timer = new Timer()
            {
                Interval = 1000
            };
            timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// Disposes of reasources held by this instance of <see cref="HostTimer"/>.
        /// </summary>
        public void Dispose()
        {
            timer.Dispose();
        }

        /// <summary>
        /// Restarts timer countdown.
        /// </summary>
        public void Restart()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// Starts timer countdown.
        /// </summary>
        public void Start()
        {
            timer.Start();
            timeRemaining = MoveTime;
        }

        /// <summary>
        /// Stops timer countdown.
        /// </summary>
        public void Stop()
        {
            timer.Stop();
        }

        /// <summary>
        /// Implements <see cref="INetworkDataProcessor.Assemble(string, object)"/>.
        /// </summary>
        /// <param name="code">Operation code.</param>
        /// <param name="data">Additional information about operation.</param>
        /// <returns>Text representation of data to be sent.</returns>
        string INetworkDataProcessor.Assemble(string code, object data)
        {
            return ClientTimer.AssembleTimeRequest(code, data);
        }

        /// <summary>
        /// Implements <see cref="INetworkDataProcessor.Process(string, string)"/>.
        /// </summary>
        /// <param name="code">Operation code.</param>
        /// <param name="body">Additional information about operation.</param>
        void INetworkDataProcessor.Process(string code, string body)
        {
        }

        /// <summary>
        /// Handles internal timer's <see cref="Timer.Tick"/> event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">An EventArgs containing event data.</param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (timeRemaining > 1)
            {
                --timeRemaining;
                Tick?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                TimePassed?.Invoke(this, EventArgs.Empty);
                Restart();
            }
        }

        /// <summary>
        /// Gets time in which move needs to be made.
        /// </summary>
        public int MoveTime
        {
            get;
        }

        /// <summary>
        /// Gets time left for making a move.
        /// </summary>
        public int TimeLeft
        {
            get
            {
                return timeRemaining;
            }
        }
    }
}
