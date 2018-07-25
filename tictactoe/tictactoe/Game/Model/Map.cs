using System;
using System.Collections.Generic;
using System.Drawing;

namespace TicTacToe.Game.Model
{
    /// <summary>
    /// Represents map to tic-tac-toe game.
    /// </summary>
    public class Map
    {
        private Size mapSize;
        private Dictionary<Point, Mark> markedPoints = new Dictionary<Point, Mark>();

        /// <summary>
        /// Occurs when a new field has been marked.
        /// </summary>
        public event MarksChangedEventHandler MarksChanged;

        /// <summary>
        /// Ocurrs when size of the map has changed.
        /// </summary>
        public event EventHandler MapSizeChanged;
        
        /// <summary>
        /// Initializes a new instance of <see cref="Map"/> with default map size of 100x100.
        /// </summary>
        public Map() :
            this(new Size(100, 100))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Map"/> with given size.
        /// </summary>
        /// <param name="mapSize">Size of the map to be created.</param>
        /// <exception cref="ArgumentException">Requested map size doesn't fit into valid range.</exception>
        public Map(Size mapSize)
        {
            MapSize = mapSize;
        }
        
        /// <summary>
        /// Checks if given field is marked.
        /// </summary>
        /// <param name="point">Field coordinate to check.</param>
        /// <returns>If given field is marked, the return value is true, otherwise it's false.</returns>
        public bool IsPointMarked(Point point)
        {
            lock (this)
            {
                return markedPoints.ContainsKey(point);
            }
        }

        /// <summary>
        /// Checks if given field coordinate lies within the map.
        /// </summary>
        /// <param name="point">Field coordinate to check.</param>
        /// <returns>If given point lies within the map the return value is true, otherwise it's false.</returns>
        public bool IsPointOnMap(Point point)
        {
            lock (this)
            {
                return point.X >= 0 && point.X < MapSize.Width && point.Y >= 0 && point.Y < MapSize.Height;
            }
        }

        /// <summary>
        /// Gets mark type in given field.
        /// </summary>
        /// <param name="point">Field coordinate to retrieve mark from.</param>
        /// <returns>Mark on the given field.</returns>
        /// <exception cref="KeyNotFoundException">Specified point is not marked.</exception>
        public Mark GetMarkOnPoint(Point point)
        {
            lock (this)
            {
                return markedPoints[point];
            }
        }

        /// <summary>
        /// Marks specified field with specified mark if the field is not already marked.
        /// </summary>
        /// <param name="point">Coordinate of a field to mark.</param>
        /// <param name="mark">Mark to put on the field.</param>
        /// <returns>If field was successfully marked, returns true. Otherwise, returns false.</returns>
        public bool MarkPoint(Point point, Mark mark)
        {
            bool ret;
            lock (this)
            {
                ret = IsPointOnMap(point) && !IsPointMarked(point);
                if (ret)
                {
                    markedPoints[point] = mark;
                }
            }

            if (ret)
            {
                MarksChanged?.Invoke(this, new MarksChangedEventArgs(point));
            }
            return ret;
        }

        /// <summary>
        /// Gets or sets size of the map.
        /// </summary>
        /// <exception cref="ArgumentException">Requested map size doesn't fit into valid range.</exception>
        public Size MapSize
        {
            get
            {
                lock (this)
                {
                    return mapSize;
                }
            }

            set
            {
                if (value.Width >= MinimumSize && value.Width <= MaximumSize)
                {
                    if (value.Height >= MinimumSize && value.Height <= MaximumSize)
                    {
                        lock (this)
                        {
                            mapSize = value;
                            markedPoints.Clear();
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Specified height exceeds acceptable range");
                    }
                }
                else
                {
                    throw new ArgumentException("Specified width exceeds acceptable range");
                }

                MapSizeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Maximum width and height of the map.
        /// </summary>
        public static readonly int MaximumSize = 1000;

        /// <summary>
        /// Minimum width and height of the map.
        /// </summary>
        public static readonly int MinimumSize = 50;
    }

    /// <summary>
    /// Provides data for <see cref="Map.MarksChanged"/> event.
    /// </summary>
    public class MarksChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of MarksChangeEventArgs.
        /// </summary>
        /// <param name="changedPoint">Field coordinate that was lastly marked.</param>
        public MarksChangedEventArgs(Point changedPoint)
        {
            ChangedPoint = changedPoint;
        }

        /// <summary>
        /// Gets coordinate of a field that was marked.
        /// </summary>
        public Point ChangedPoint
        {
            get;
        }
    }

    /// <summary>
    /// Represents method for handling <see cref="Map.MarksChanged"/> event.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">A MarksChangedEventArgs containing event data.</param>
    public delegate void MarksChangedEventHandler(object sender, MarksChangedEventArgs e);
}
