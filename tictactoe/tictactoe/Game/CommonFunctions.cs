using System;
using System.Collections.Generic;
using System.ComponentModel;
using TicTacToe.Game.Model;
using System.Drawing;

namespace TicTacToe.Game
{
    /// <summary>
    /// Container for some commonly needed functions.
    /// </summary>
    static class CommonFunctions
    {
        /// <summary>
        /// Draws specified mark sign in specific area.
        /// </summary>
        /// <param name="graphics">Graphics object for drawing operations.</param>
        /// <param name="mark">Mark to draw.</param>
        /// <param name="rect">Area into which mark will be drawn.</param>
        public static void DrawMark(Graphics graphics, Mark mark, Rectangle rect)
        {
            using (Pen pen = new Pen(mark == Mark.Cross ? Color.Red : Color.Green, 3.0f))
            {
                DrawMark(graphics, mark, rect, pen);
            }
        }

        /// <summary>
        /// Draws specified mark sign in specific area using specific pen.
        /// </summary>
        /// <param name="graphics">Graphics object for drawing operations.</param>
        /// <param name="mark">Mark to draw.</param>
        /// <param name="rect">Area into which mark will be drawn.</param>
        /// <param name="pen">Pen that will be using for drawing.</param>
        public static void DrawMark(Graphics graphics, Mark mark, Rectangle rect, Pen pen)
        {
            if (mark == Mark.Cross)
            {
                graphics.DrawLine(pen, rect.Location, new Point(rect.Right, rect.Bottom));
                graphics.DrawLine(pen, new Point(rect.Right, rect.Top), new Point(rect.Left, rect.Bottom));
            }
            else if (mark == Mark.Nought)
            {
                graphics.DrawEllipse(pen, rect);
            }
            else
            {
                throw new InvalidEnumArgumentException("mark", (int)mark, typeof(Mark));
            }
        }
    }
}
