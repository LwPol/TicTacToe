using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Game.View;
using System.Windows.Forms;
using System.Drawing;

namespace TicTacToe.Game.Controller
{
    /// <summary>
    /// Reprezents animation played after one of the players wins.
    /// </summary>
    public class WinningAnimation : IDisposable
    {
        private Timer timer;
        private MapView mapView;
        private WinConditionMetEventArgs args;
        private int combinationLength;

        private int time;
        private int animationTime;

        /// <summary>
        /// Occurs when animation is ended.
        /// </summary>
        public event EventHandler AnimationStopped;

        /// <summary>
        /// Initializes a new instance of <see cref="WinningAnimation"/> with given
        /// <see cref="MapView"/> and winning data.
        /// </summary>
        /// <param name="view"><see cref="MapView"/> object to perform animation on.</param>
        /// <param name="args">A <see cref="WinConditionMetEventArgs"/> containing
        /// <see cref="GameController.WinConditionMet"/> event data.</param>
        public WinningAnimation(MapView view, WinConditionMetEventArgs args)
        {
            mapView = view;
            this.args = args;
            

            timer = new Timer();
        }

        /// <summary>
        /// Disposes resources used by this object.
        /// </summary>
        public void Dispose()
        {
            timer.Dispose();
        }

        /// <summary>
        /// Start the animation.
        /// </summary>
        public void Start()
        {
            time = 0;
            if (animationTime > 0)
            {
                timer.Tick += Update;
                mapView.Paint += UpdatePaint;
                timer.Start();
            }
        }

        /// <summary>
        /// Performs painting during animation.
        /// </summary>
        /// <param name="graphics">Graphics object used for drawing operations.</param>
        private void Paint(Graphics graphics)
        {
            using (Pen pen = new Pen(args.WinningMark == Mark.Cross ? Color.Red : Color.Green, 5.0f))
            {
                Point start = mapView.GetTilePositionInClientSpace(args.CombinationStartingPoint);
                Size tileSizeInViewport = mapView.TileSizeInViewport;
                Point destination = new Point();

                switch (args.Direction)
                {
                    case WinningCombinationDirection.Horizontal:
                        start.Y += tileSizeInViewport.Height / 2;
                        
                        destination = new Point(start.X + CombinationLength * tileSizeInViewport.Width,
                            start.Y);
                        break;
                    case WinningCombinationDirection.Diagonal:
                        destination = new Point(start.X + CombinationLength * tileSizeInViewport.Width,
                            start.Y + CombinationLength * tileSizeInViewport.Height);
                        break;
                    case WinningCombinationDirection.Vertical:
                        start.X += tileSizeInViewport.Width / 2;
                        
                        destination = new Point(start.X,
                            start.Y + CombinationLength * tileSizeInViewport.Height);
                        break;
                    case WinningCombinationDirection.AntiDiagonal:
                        start.X += tileSizeInViewport.Width;

                        destination = new Point(start.X - CombinationLength * tileSizeInViewport.Width,
                            start.Y + CombinationLength * tileSizeInViewport.Height);
                        break;
                }

                double animationRatio = (double)time / animationTime;
                Point end = new Point(start.X + (int)((destination.X - start.X) * animationRatio),
                    start.Y + (int)((destination.Y - start.Y) * animationRatio));

                graphics.DrawLine(pen, start, end);
            }
        }

        /// <summary>
        /// Handles timer Tick event forcing MapView to redraw.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Empty EventArgs.</param>
        private void Update(object sender, EventArgs e)
        {
            time += timer.Interval;
            if (time > animationTime)
            {
                time = animationTime;
            }
            
            mapView.Invalidate();

            if (time >= animationTime)
            {
                timer.Tick -= Update;
                timer.Stop();
                AnimationStopped?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles MapView's Paint event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">A <see cref="PaintEventArgs"/> containing event data.</param>
        private void UpdatePaint(object sender, PaintEventArgs e)
        {
            Paint(e.Graphics);
        }

        /// <summary>
        /// Gets or sets total time of animation duration.
        /// </summary>
        public int AnimationTime
        {
            get
            {
                return animationTime;
            }

            set
            {
                if (value > 0 && !timer.Enabled)
                {
                    animationTime = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets length of winning combination.
        /// </summary>
        public int CombinationLength
        {
            get
            {
                return combinationLength;
            }

            set
            {
                if (!timer.Enabled)
                {
                    combinationLength = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets timer interval.
        /// </summary>
        public int Interval
        {
            get
            {
                return timer.Interval;
            }

            set
            {
                timer.Interval = value;
            }
        }
    }
}
