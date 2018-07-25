using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using TicTacToe.Game.Model;
using System.Drawing.Drawing2D;
using TicTacToe.Game;

namespace TicTacToe.Game.View
{
    /// <summary>
    /// Provides control for displaying and interacting with map to tic-tac-toe game.
    /// </summary>
    public class MapView : Control
    {
        private Point viewLocation;
        private double zoom = 1.0;

        private Size mapSize;

        private Dictionary<Point, Mark> markedPoints = new Dictionary<Point, Mark>();

        private Point hover = new Point(-1, -1);
        private Point lastMousePos;
        private bool rightButtonDown = false;
        
        private sbyte lastMarkedTileStatus = showLastMarkedTile | showIndicatorToLastlyMarkedTile;
        private Point lastMarked = new Point(-1, -1);
        private const sbyte showLastMarkedTile = 0x01;
        private const sbyte showIndicatorToLastlyMarkedTile = 0x02;

        private const int tileWidth = 200;
        private const int tileHeight = 200;
        private const int tileMargin = 10;

        /// <summary>
        /// Occurs when <see cref="MapView"/> is clicked with left mouse button.
        /// </summary>
        public event MapViewClickEventHandler MapViewClick;

        /// <summary>
        /// Initializes a new instance of <see cref="MapView"/> class with default size of map of 100x100.
        /// </summary>
        public MapView() :
            this(new Size(100, 100))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MapView"/> class with specified size of map.
        /// </summary>
        /// <param name="mapSize">Size of underlying map.</param>
        public MapView(Size mapSize)
        {
            this.mapSize = mapSize;
            DoubleBuffered = true;
        }

        /// <summary>
        /// Centers view of the map at given tile.
        /// </summary>
        /// <param name="tileCoord">Coordinates of a tile that view will be centered at.</param>
        public void CenterOnTile(Point tileCoord)
        {
            Size viewportSize = ViewportSize;

            Point middleTileUpperLeftCorner = new Point(TileSize.Width * tileCoord.X,
                TileSize.Height * tileCoord.Y);
            Point middleTileCenter = new Point(middleTileUpperLeftCorner.X + TileSize.Width / 2,
                middleTileUpperLeftCorner.Y + TileSize.Height / 2);

            ViewLocation = new Point(middleTileCenter.X - viewportSize.Width / 2,
                middleTileCenter.Y - viewportSize.Height / 2);
        }

        /// <summary>
        /// Computes the location of the specified client point into map coordinates.
        /// </summary>
        /// <param name="point">The client coordinate Point to convert.</param>
        /// <returns>Point in map coordinates.</returns>
        public Point ControlToMapCoords(Point point)
        {
            return new Point(ViewLocation.X + (int)(point.X / Zoom),
                ViewLocation.Y + (int)(point.Y / Zoom));
        }

        /// <summary>
        /// Computes the location of the specified client point into tile grid coordinates.
        /// </summary>
        /// <param name="point">The client coordinate <see cref="Point"/> to convert</param>
        /// <returns>Tile coordinate in grid if Point refers to one. Otherwise null.</returns>
        public Point? ControlToTileCoords(Point point)
        {
            Point mapCoords = ControlToMapCoords(point);
            Point tileCoord = new Point(mapCoords.X / tileWidth, mapCoords.Y / tileHeight);

            if (IsTileInMap(tileCoord))
            {
                Rectangle bounds = new Rectangle(tileCoord.X * tileWidth + tileMargin,
                    tileCoord.Y * tileHeight + tileMargin,
                    tileWidth - 2 * tileMargin,
                    tileHeight - 2 * tileMargin);

                if (bounds.Contains(mapCoords))
                {
                    return tileCoord;
                }
            }

            return null;
        }

        /// <summary>
        /// Computes the location of upperleft corner of given tile into client coordinates.
        /// </summary>
        /// <param name="tile">Tile in grid coordinates to compute upperleft corner location for.</param>
        /// <returns><see cref="Point"/> in client coordinates.</returns>
        public Point GetTilePositionInClientSpace(Point tile)
        {
            Size tileInView = TileSizeInViewport;
            Point viewLocationScaled = new Point((int)(ViewLocation.X * Zoom), (int)(ViewLocation.Y * Zoom));
            return TransformByTranslation(new Point(tile.X * tileInView.Width, tile.Y * tileInView.Height),
                viewLocationScaled);
        }

        /// <summary>
        /// Gets coordinates in client space of indicator triangle to last marked tile.
        /// </summary>
        /// <param name="pointerHeight">Height of indicator, in pixels.</param>
        /// <param name="pointerWidth">Width of indicator, in pixels.</param>
        /// <returns>Returns table of 3 points making indicator triangle or null if last
        /// marked tile is in viewport and indicator is not needed.</returns>
        public Point[] GetIndicatorPointsToLastMarkedTile(int pointerHeight, int pointerWidth)
        {
            if (lastMarked == new Point(-1, -1))
            {
                return null;
            }

            Rectangle lastMarkedTile = new Rectangle(GetTilePositionInClientSpace(lastMarked), TileSizeInViewport);
            if (DisplayRectangle.IntersectsWith(lastMarkedTile))
            {
                return null;
            }

            Point[] result = new Point[3];
            result[1] = new Point(-pointerHeight, pointerWidth / 2);
            result[2] = new Point(-pointerHeight, -pointerWidth / 2);

            Point viewToTileVector = new Point(lastMarkedTile.X + lastMarkedTile.Width / 2 - Width / 2,
                lastMarkedTile.Y + lastMarkedTile.Height / 2 - Height / 2);
            double viewToTileLength =
                Math.Sqrt(viewToTileVector.X * viewToTileVector.X + viewToTileVector.Y * viewToTileVector.Y);
            double xAxisAngleCos = viewToTileVector.X / viewToTileLength;
            double xAxisAngleSin = viewToTileVector.Y / viewToTileLength;

            // rotate points
            for (int i = 1; i < result.Length; ++i)
            {
                result[i] = new Point((int)(result[i].X * xAxisAngleCos - result[i].Y * xAxisAngleSin),
                    (int)(result[i].X * xAxisAngleSin + result[i].Y * xAxisAngleCos));
            }

            const int boundDistance = 20;

            int[] vecProductsSigns =
            {
                CrossProductSign(viewToTileVector, new Point(Width / 2, Height / 2)),
                CrossProductSign(viewToTileVector, new Point(-Width / 2, Height / 2)),
                CrossProductSign(viewToTileVector, new Point(-Width / 2, -Height / 2)),
                CrossProductSign(viewToTileVector, new Point(Width / 2, -Height / 2))
            };
            Point indicatorLineBeg, indicatorLineDir;
            if (vecProductsSigns[0] < 0 && vecProductsSigns[1] >= 0 &&
                vecProductsSigns[2] > 0 && vecProductsSigns[3] < 0)
            {
                indicatorLineBeg = new Point(boundDistance, Height - boundDistance);
                indicatorLineDir = new Point(1, 0);
            }
            else if (vecProductsSigns[0] < 0 && vecProductsSigns[1] < 0 &&
                vecProductsSigns[2] >= 0 && vecProductsSigns[3] > 0)
            {
                indicatorLineBeg = new Point(boundDistance, boundDistance);
                indicatorLineDir = new Point(0, 1);
            }
            else if (vecProductsSigns[0] > 0 && vecProductsSigns[1] < 0 &&
                vecProductsSigns[2] < 0 && vecProductsSigns[3] >= 0)
            {
                indicatorLineBeg = new Point(boundDistance, boundDistance);
                indicatorLineDir = new Point(1, 0);
            }
            else
            {
                indicatorLineBeg = new Point(Width - boundDistance, boundDistance);
                indicatorLineDir = new Point(0, 1);
            }

            int mainDeterminant = viewToTileVector.X * indicatorLineDir.Y -
                viewToTileVector.Y * indicatorLineDir.X;

            int tDeterminant = (indicatorLineBeg.X - Width / 2) * indicatorLineDir.Y -
                (indicatorLineBeg.Y - Height / 2) * indicatorLineDir.X;
            double t0 = (double)tDeterminant / mainDeterminant;
            result[0] = new Point((int)(Width / 2 + viewToTileVector.X * t0),
                (int)(Height / 2 + viewToTileVector.Y * t0));

            for (int i = 1; i < result.Length; ++i)
            {
                result[i].X += result[0].X;
                result[i].Y += result[0].Y;
            }

            return result;
        }

        /// <summary>
        /// Checks wheter given tile coordinates lie within grid of the underlying map.
        /// </summary>
        /// <param name="point">Tile grid coordinate to check.</param>
        /// <returns>True if tile lies within grid of the map. Otherwise false.</returns>
        public bool IsTileInMap(Point point)
        {
            return point.X >= 0 && point.X < mapSize.Width && point.Y >= 0 && point.Y < mapSize.Height;
        }

        /// <summary>
        /// Marks point on underlying map with cross or nought.
        /// </summary>
        /// <param name="point"><see cref="Point"/> in tile grid coordinates to mark.</param>
        /// <param name="mark"><see cref="Mark"/> to make (cross or nought).</param>
        /// <remarks>If given tile was previosly marked the previous mark is overriden.</remarks>
        public void MarkPoint(Point point, Mark mark)
        {
            if (IsTileInMap(point))
            {
                markedPoints[lastMarked = point] = mark;
                Invalidate();
            }
        }

        /// <summary>
        /// Translates viewport by given vector.
        /// </summary>
        /// <param name="translationVector">Vector to translate viewport by.</param>
        /// <remarks>It's not possible to translate viewport outside of map bounds with this function.</remarks>
        public void TranslateView(Point translationVector)
        {
            ViewLocation = new Point(ViewLocation.X + translationVector.X, ViewLocation.Y + translationVector.Y);
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseDown"/> event.
        /// </summary>
        /// <param name="e"><see cref="MouseEventArgs"/> that contains info about event.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                lastMousePos = e.Location;
                rightButtonDown = true;
            }

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseLeave"/> event.
        /// </summary>
        /// <param name="e"><see cref="EventArgs"/> containing info about event.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            Hover = new Point(-1, -1);
            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseMove"/> event.
        /// </summary>
        /// <param name="e"><see cref="MouseEventArgs"/> containing info about event.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (rightButtonDown)
            {
                UpdateView(e.Location);
            }
            else
            {
                UpdateHoverState(e.Location);
            }

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseUp"/> event.
        /// </summary>
        /// <param name="e"><see cref="MouseEventArgs"/> containing info about event.</param>
        /// <remarks>Depending on mouse position might raise also <see cref="MapViewClick"/> event before <see cref="Control.MouseUp"/> event.</remarks>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                rightButtonDown = false;
            }
            else if (e.Button == MouseButtons.Left)
            {
                Point? tileCoord = ControlToTileCoords(e.Location);
                if (tileCoord.HasValue)
                {
                    MapViewClick?.Invoke(this, new MapViewClickEventArgs(tileCoord.Value));
                }
            }

            base.OnMouseUp(e);
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseWheel"/> event.
        /// </summary>
        /// <param name="e"><see cref="MouseEventArgs"/> containing info about event.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            Zoom += (double)e.Delta / 120 * 0.1;
            base.OnMouseWheel(e);
        }

        /// <summary>
        /// Raises the <see cref="Control.Paint"/> event.
        /// </summary>
        /// <param name="e"><see cref="PaintEventArgs"/> containing info about event.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            HandlePaintEvent(e);
            base.OnPaint(e);
        }

        /// <summary>
        /// Auxilary function calculating sign of Z component of vector product.
        /// </summary>
        /// <param name="lhs">Left hand operand of vector product.</param>
        /// <param name="rhs">Right hand operand of vector product.</param>
        /// <returns>Sign (positive or negative number or zero) of Z component of vector product.</returns>
        private static int CrossProductSign(Point lhs, Point rhs)
        {
            int zProduct = lhs.X * rhs.Y - lhs.Y * rhs.X;
            return zProduct > 0 ? 1 : (zProduct < 0 ? -1 : 0);
        }

        /// <summary>
        /// Draws indicator to lastly marked tile on map if the marked tile is out of viewport.
        /// </summary>
        /// <param name="graphics">Graphics object to use for drawing operations.</param>
        private void DrawIndicatorToLastMarkedPoint(Graphics graphics)
        {
            var pt = GetIndicatorPointsToLastMarkedTile(20, 50);
            if (pt != null)
            {
                graphics.FillPolygon(Brushes.Yellow, pt);
            }
        }

        /// <summary>
        /// Draws underlying map on control.
        /// </summary>
        /// <param name="e">PaintEventArgs containing info about Paint event.</param>
        private void HandlePaintEvent(PaintEventArgs e)
        {
            // draw white background
            e.Graphics.FillRectangle(Brushes.White, DisplayRectangle);

            // calcaulations...
            Size tileInView = TileSizeInViewport;
            Size mapSizeInView = new Size(tileInView.Width * MapSize.Width, tileInView.Height * MapSize.Height);
            Point viewLocationScaled = new Point((int)(ViewLocation.X * Zoom),
                (int)(ViewLocation.Y * Zoom));
            Rectangle mapInViewCoords =
                new Rectangle(TransformByTranslation(new Point(0, 0), viewLocationScaled), mapSizeInView);

            Point upperLeftCornerTile =
                new Point((viewLocationScaled.X + tileInView.Width - 1) / tileInView.Width * tileInView.Width,
                    (viewLocationScaled.Y + tileInView.Height - 1) / tileInView.Height * tileInView.Height);

            // highlight last marked tile
            if (ShowLastMarkedTile && lastMarked != new Point(-1, -1))
            {
                Rectangle marked = new Rectangle(
                    TransformByTranslation(new Point(lastMarked.X * tileInView.Width,
                        lastMarked.Y * tileInView.Height),
                        viewLocationScaled),
                    tileInView
                );
                
                Rectangle[] rects = new Rectangle[4];

                Size regionsSize = new Size(marked.Width / 2, marked.Height / 2);
                rects[0] = new Rectangle(marked.Location, regionsSize);
                rects[1] = new Rectangle(new Point(marked.Location.X + regionsSize.Width, marked.Location.Y),
                    regionsSize);
                rects[2] = new Rectangle(new Point(marked.Location.X, marked.Location.Y + regionsSize.Height),
                    regionsSize);
                rects[3] = new Rectangle(new Point(marked.Location.X + regionsSize.Width,
                    marked.Location.Y + regionsSize.Height),
                    regionsSize);

                float[] orientations =
                {
                    45.0f, 135.0f, 315.0f, 225.0f
                };

                for (int i = 0; i < rects.Length; ++i)
                {
                    using (Brush brush =
                        new LinearGradientBrush(rects[i], Color.White, Color.Yellow, orientations[i]))
                    {
                        e.Graphics.FillRectangle(brush, rects[i]);
                    }
                }
            }

            // drawing lines
            for (Point pt = TransformByTranslation(upperLeftCornerTile, viewLocationScaled); ;)
            {
                bool outOfView = true;

                if (pt.X <= Width && pt.X <= mapInViewCoords.Right)
                {
                    e.Graphics.DrawLine(Pens.Black,
                        new Point(pt.X, mapInViewCoords.Top),
                        new Point(pt.X, mapInViewCoords.Bottom));
                    outOfView = false;
                }

                if (pt.Y <= Height && pt.Y <= mapInViewCoords.Bottom)
                {
                    e.Graphics.DrawLine(Pens.Black,
                        new Point(mapInViewCoords.Left, pt.Y),
                        new Point(mapInViewCoords.Right, pt.Y));
                    outOfView = false;
                }

                if (outOfView)
                    break;

                pt.X += tileInView.Width;
                pt.Y += tileInView.Height;
            }

            // draw marks in tiles
            Pen greenPen = new Pen(Color.Green, 3.0f);
            Pen redPen = new Pen(Color.Red, 3.0f);

            Point upperLeftTile = new Point(upperLeftCornerTile.X / tileInView.Width,
                upperLeftCornerTile.Y / tileInView.Height);
            Point rightBottomTile = new Point(Width / tileInView.Width + upperLeftTile.X,
                Height / tileInView.Height + upperLeftTile.Y);

            if (upperLeftCornerTile.X != viewLocationScaled.X)
            {
                --upperLeftTile.X;
            }
            if (upperLeftCornerTile.Y != viewLocationScaled.Y)
            {
                --upperLeftTile.Y;
            }

            for (int i = upperLeftTile.X; i <= rightBottomTile.X; ++i)
            {
                for (int j = upperLeftTile.Y; j <= rightBottomTile.Y; ++j)
                {
                    if (markedPoints.TryGetValue(new Point(i, j), out Mark mark))
                    {
                        Point location = TransformByTranslation(
                            new Point(i * tileInView.Width + tileMargin, j * tileInView.Height + tileMargin),
                            viewLocationScaled
                        );
                        Rectangle bounds = new Rectangle(location, new Size(tileInView.Width - 2 * tileMargin,
                            tileInView.Height - 2 * tileMargin));
                        CommonFunctions.DrawMark(e.Graphics, mark, bounds);
                    }
                }
            }

            greenPen.Dispose();
            redPen.Dispose();

            // draw hovering tile
            if (Hover != new Point(-1, -1))
            {
                Point location = TransformByTranslation(
                    new Point(Hover.X * tileInView.Width + tileMargin, Hover.Y * tileInView.Height + tileMargin),
                    viewLocationScaled
                );
                Rectangle bounds = new Rectangle(location, new Size(tileInView.Width - 2 * tileMargin,
                    tileInView.Height - 2 * tileMargin));

                e.Graphics.DrawRectangle(Pens.Gray, bounds);
            }

            // draw indicator to lastly marked point if necessary
            if (ShowIndicatorToLastlyMarkedTile)
            {
                DrawIndicatorToLastMarkedPoint(e.Graphics);
            }
        }

        /// <summary>
        /// Auxilary function that subtracts points.
        /// </summary>
        /// <param name="point">Point to subtract from.</param>
        /// <param name="basePoint">Point to subtract.</param>
        /// <returns>Result of subtraction of two points.</returns>
        private static Point TransformByTranslation(Point point, Point basePoint)
        {
            return new Point(point.X - basePoint.X, point.Y - basePoint.Y);
        }

        /// <summary>
        /// Sets tile which mouse cursor is hovering over.
        /// </summary>
        /// <param name="hoverPoint">Point in client coordinates of the cursor.</param>
        private void UpdateHoverState(Point hoverPoint)
        {
            Point? tileCoord = ControlToTileCoords(hoverPoint);
            if (tileCoord.HasValue)
            {
                Hover = tileCoord.Value;
            }
            else
            {
                Hover = new Point(-1, -1);
            }
        }

        /// <summary>
        /// Translates viewport according to mouse moves.
        /// </summary>
        /// <param name="point">Last position of the cursor in client coordiantes.</param>
        private void UpdateView(Point point)
        {
            Point translationControlCoords = new Point(lastMousePos.X - point.X, lastMousePos.Y - point.Y);
            TranslateView(new Point((int)(translationControlCoords.X / Zoom),
                (int)(translationControlCoords.Y / Zoom)));
            lastMousePos = point;
        }

        /// <summary>
        /// Gets or sets information about displaying indicator pointing to lastly marked tile if
        /// this tile is outside of viewport.
        /// </summary>
        public bool ShowIndicatorToLastlyMarkedTile
        {
            get
            {
                return (lastMarkedTileStatus & showIndicatorToLastlyMarkedTile) != 0;
            }

            set
            {
                if (value)
                {
                    lastMarkedTileStatus |= showIndicatorToLastlyMarkedTile;
                }
                else
                {
                    lastMarkedTileStatus &= ~showIndicatorToLastlyMarkedTile;
                }
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets information about highlighting and emphasizing lastly made mark.
        /// </summary>
        public bool ShowLastMarkedTile
        {
            get
            {
                return (lastMarkedTileStatus & showLastMarkedTile) != 0;
            }

            set
            {
                if (value)
                {
                    lastMarkedTileStatus |= showLastMarkedTile;
                }
                else
                {
                    lastMarkedTileStatus &= ~showLastMarkedTile;
                }
                Invalidate();
            }
        }

        /// <summary>
        /// Gets size of a single tile on map.
        /// </summary>
        public Size TileSize
        {
            get
            {
                return new Size(tileWidth, tileHeight);
            }
        }

        /// <summary>
        /// Gets size of a single tile as it is seen in viewport.
        /// </summary>
        public Size TileSizeInViewport
        {
            get
            {
                return new Size((int)(tileWidth * Zoom), (int)(tileHeight * Zoom));
            }
        }

        /// <summary>
        /// Gets or sets location of upperleft corner of viewport. Viewport cannot lie outside of map bounds.
        /// </summary>
        public Point ViewLocation
        {
            get
            {
                return viewLocation;
            }

            set
            {
                Size totalMapSize = new Size(tileWidth * MapSize.Width, tileHeight * MapSize.Height);
                Size viewSize = new Size((int)(Width / Zoom), (int)(Height / Zoom));
                Rectangle possibleViewLocations = new Rectangle(0, 0,
                    totalMapSize.Width > viewSize.Width ? totalMapSize.Width - viewSize.Width : 0,
                    totalMapSize.Height > viewSize.Height ? totalMapSize.Height - viewSize.Height : 0);

                if (possibleViewLocations.Contains(value))
                {
                    viewLocation = value;
                }
                else
                {
                    if (value.X >= possibleViewLocations.Right)
                    {
                        viewLocation.X = possibleViewLocations.Right;
                    }
                    else if (value.X < 0)
                    {
                        viewLocation.X = 0;
                    }
                    else
                    {
                        viewLocation.X = value.X;
                    }

                    if (value.Y >= possibleViewLocations.Bottom)
                    {
                        viewLocation.Y = possibleViewLocations.Bottom;
                    }
                    else if (value.Y < 0)
                    {
                        viewLocation.Y = 0;
                    }
                    else
                    {
                        viewLocation.Y = value.Y;
                    }
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Gets size of portion of map that can be seen in viewport.
        /// </summary>
        public Size ViewportSize
        {
            get
            {
                return new Size((int)(Width / Zoom), (int)(Height / Zoom));
            }
        }

        /// <summary>
        /// Gets or sets zoom of viewport. Conforms ViewLocation if necessary.
        /// </summary>
        public double Zoom
        {
            get
            {
                return zoom;
            }

            set
            {
                const double maximumZoom = 1.5, minimumZoom = 0.3;
                if (value >= minimumZoom && value <= maximumZoom)
                {
                    zoom = value;
                }
                else if (value < minimumZoom)
                {
                    zoom = minimumZoom;
                }
                else
                {
                    zoom = maximumZoom;
                }

                ViewLocation = ViewLocation;
            }
        }

        /// <summary>
        /// Gets or sets size of map in tiles.
        /// </summary>
        public Size MapSize
        {
            get
            {
                return mapSize;
            }

            set
            {
                if (value.Height > 0 && value.Width > 0)
                {
                    markedPoints.Clear();
                    mapSize = value;
                    ViewLocation = new Point(0, 0);
                }
            }
        }

        /// <summary>
        /// Gets or sets tile in grid coordinates over which mouse cursor is hovering.
        /// </summary>
        private Point Hover
        {
            get
            {
                return hover;
            }

            set
            {
                if (hover != value)
                {
                    hover = value;
                    Invalidate();
                }
            }
        }
    }

    /// <summary>
    /// Provides data for MapViewClick event from MapView class.
    /// </summary>
    public class MapViewClickEventArgs : EventArgs
    {

        /// <summary>
        /// Initializes a new instance of MapViewClickEventArgs class.
        /// </summary>
        /// <param name="location">Clicked tile location in map's grid.</param>
        public MapViewClickEventArgs(Point location)
        {
            Location = location;
        }

        /// <summary>
        /// Gets clicked tile location in map's grid.
        /// </summary>
        public Point Location { get; }
    }

    /// <summary>
    /// Represents the method for handling MapViewClick event from <see cref="MapView"/> class.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">A <see cref="MapViewClickEventArgs"/> that contains the event data.</param>
    public delegate void MapViewClickEventHandler(object sender, MapViewClickEventArgs e);

}
