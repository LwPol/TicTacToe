using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace TicTacToe.Game.Controller
{
    /// <summary>
    /// Data structure for MarkAck operation.
    /// </summary>
    public struct MarkAckInfo
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MarkAckInfo"/> with specified point and mark.
        /// </summary>
        /// <param name="point">Point for which mark operation has been acknowledged.</param>
        /// <param name="mark">Mark on the specified point.</param>
        public MarkAckInfo(Point point, Mark mark)
        {
            Point = point;
            Mark = mark;
        }

        /// <summary>
        /// Converts string representation of <see cref="MarkAckInfo"/> to a new instance of <see cref="MarkAckInfo"/>.
        /// </summary>
        /// <param name="s">String to convert.</param>
        /// <returns><see cref="MarkAckInfo"/> being result of conversion.</returns>
        /// <exception cref="FormatException">Format of string does not match any
        /// <see cref="MarkAckInfo"/> representation.</exception>
        public static MarkAckInfo Parse(string s)
        {
            Regex regex = new Regex("\\d+ \\d+ [cn]");
            MatchCollection matches = regex.Matches(s);
            if (matches.Count != 1 || matches[0].Index != 0)
                throw new FormatException();

            Match match = matches[0];
            string[] tokens = match.Value.Split(' ');
            try
            {
                Point point = new Point(int.Parse(tokens[0]), int.Parse(tokens[1]));
                Mark mark = tokens[2][0] == 'c' ? Mark.Cross : Mark.Nought;
                return new MarkAckInfo(point, mark);
            }
            catch (Exception)
            {
                throw new FormatException();
            }
        }

        /// <summary>
        /// Overrides <see cref="Object.ToString"/>.
        /// </summary>
        /// <returns>String reprentation of this <see cref="MarkAckInfo"/> instance.</returns>
        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Point.X, Point.Y, Mark == Mark.Cross ? 'c' : 'n');
        }

        public Mark Mark
        {
            get; set;
        }

        public Point Point
        {
            get; set;
        }
    }
}
