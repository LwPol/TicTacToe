using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using TicTacToe.Game.Model;
using TicTacToe.Game;

namespace TicTacToe.Game.View
{
    /// <summary>
    /// Provides display for graphical representation of marks.
    /// </summary>
    public class MarkDisplay : Control
    {
        private Mark current;

        /// <summary>
        /// Raises <see cref="Control.Paint"/> event. (Overrides <see cref="Control.OnPaint(PaintEventArgs)"/>)
        /// </summary>
        /// <param name="e">A <see cref="PaintEventArgs"/> containing event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.White, DisplayRectangle);

            Rectangle drawArea = new Rectangle(DisplayRectangle.Left + 2, DisplayRectangle.Top + 1,
                DisplayRectangle.Width - 4, DisplayRectangle.Height - 4);
            CommonFunctions.DrawMark(e.Graphics, Current, drawArea);
            base.OnPaint(e);
        }

        /// <summary>
        /// Gets or sets mark displayed in the control.
        /// </summary>
        public Mark Current
        {
            get
            {
                return current;
            }

            set
            {
                current = value;
                Invalidate();
            }
        }
    }
}
