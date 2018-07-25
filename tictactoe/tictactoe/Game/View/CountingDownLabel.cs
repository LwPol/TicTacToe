using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace TicTacToe.Game.View
{
    /// <summary>
    /// Provides label displaying counting-down time.
    /// </summary>
    public class CountingDownLabel : Label
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CountingDownLabel"/>.
        /// </summary>
        public CountingDownLabel()
        {
            ChangingColorTime = 5;
            HotColor = Color.Red;
            NormalColor = Color.Blue;
        }

        /// <summary>
        /// Raises <see cref="Control.Paint"/> event (overrides <see cref="Control.OnPaint(PaintEventArgs)"/>).
        /// </summary>
        /// <param name="e">A <see cref="PaintEventArgs"/> containing event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            ForeColor = Time < TimeSpan.FromSeconds(ChangingColorTime) ? HotColor : NormalColor;
            base.OnPaint(e);
        }

        /// <summary>
        /// Gets or sets time in seconds after which label changes color from normal to hot.
        /// </summary>
        public int ChangingColorTime { get; set; }

        /// <summary>
        /// Gets or sets color of label when time displayed is less than <see cref="ChangingColorTime"/>.
        /// </summary>
        public Color HotColor { get; set; }

        /// <summary>
        /// Gets or sets color of label when time displayed is greater than or equal to <see cref="ChangingColorTime"/>.
        /// </summary>
        public Color NormalColor { get; set; }

        /// <summary>
        /// Gets or sets time displayed by this label.
        /// </summary>
        public TimeSpan Time
        {
            get
            {
                try
                {
                    return TimeSpan.Parse("00:" + Text);
                }
                catch (Exception)
                {
                    return new TimeSpan();
                }
            }

            set
            {
                Text = string.Format("{0:00}:{1:00}", (int)value.TotalMinutes, value.Seconds);
            }
        }
    }
}
