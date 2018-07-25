using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TicTacToe.Game.Controller;

namespace TicTacToe.Game.Model
{
    /// <summary>
    /// Represents timer counting down time to move at clientside. Timing events are raised based
    /// on appropriate method calls from external source.
    /// </summary>
    public class ClientTimer : ITiming, INetworkDataProcessor
    {
        /// <summary>
        /// Occurs when one second has passed since last timer tick.
        /// </summary>
        public event EventHandler Tick;
        /// <summary>
        /// Occurs when time for making a move has passed.
        /// </summary>
        public event EventHandler TimePassed;

        /// <summary>
        /// Initializes a new instance of <see cref="ClientTimer"/> with specified time for making a move.
        /// </summary>
        /// <param name="moveTime"></param>
        public ClientTimer(int moveTime)
        {
            MoveTime = TimeLeft = moveTime;
        }

        /// <summary>
        /// Restarts the timer.
        /// </summary>
        public void Restart()
        {
            Start();
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start()
        {
            TimeLeft = MoveTime;
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Assembles data about particular time actions.
        /// </summary>
        /// <param name="code">Time action code.</param>
        /// <param name="data">Additional information about operation.</param>
        /// <returns>Text representation of time actions.</returns>
        internal static string AssembleTimeRequest(string code, object data)
        {
            if (code == TimeCode)
            {
                return code + "\n" + (string)data;
            }
            else if (code == TimePassedCode)
            {
                return code;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Implements <see cref="INetworkDataProcessor.Assemble(string, object)"/>.
        /// </summary>
        /// <param name="code">Operation code.</param>
        /// <param name="data">Additional information about operation.</param>
        /// <returns>Text representation of data to be sent.</returns>
        string INetworkDataProcessor.Assemble(string code, object data)
        {
            return AssembleTimeRequest(code, data);
        }

        /// <summary>
        /// Implements <see cref="INetworkDataProcessor.Process(string, string)"/>.
        /// </summary>
        /// <param name="code">Operation code.</param>
        /// <param name="body">Additional information about operation.</param>
        void INetworkDataProcessor.Process(string code, string body)
        {
            if (code == TimeCode)
            {
                if (TimeSpan.TryParse(body, out TimeSpan syncTime))
                {
                    TimeLeft = (int)syncTime.TotalSeconds;
                    Tick?.Invoke(this, EventArgs.Empty);
                }
            }
            else if (code == TimePassedCode)
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
        /// Gets or sets time left for making a move.
        /// </summary>
        public int TimeLeft
        {
            get; set;
        }

        /// <summary>
        /// Code for time tick operation.
        /// </summary>
        public static readonly string TimeCode = "time";

        /// <summary>
        /// Code for time passed operation.
        /// </summary>
        public static readonly string TimePassedCode = "time_passed";
    }
}
