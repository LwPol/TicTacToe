using System;
using System.Threading;
using TicTacToe.Game.Model;
using System.Drawing;
using System.ComponentModel;

namespace TicTacToe.Game.Controller
{
    /// <summary>
    /// Represents controller handling game logic.
    /// </summary>
    public class GameController
    {
        private ITiming timing;

        /// <summary>
        /// Occurs when one of the players wins.
        /// </summary>
        public event WinConditionMetEventHandler WinConditionMet;

        /// <summary>
        /// Initializes a new instance of <see cref="GameController"/> with given map size.
        /// </summary>
        /// <param name="mapSize">Size of the map to play with.</param>
        public GameController(Size mapSize)
        {
            State = new GameState()
            {
                Current = Mark.Cross,
                TicTacToeMap = new Map(mapSize)
            };
        }

        /// <summary>
        /// Initializes a new instance of <see cref="GameController"/> with given map
        /// size and <see cref="ITiming"/> object.
        /// </summary>
        /// <param name="mapSize">Size of the map to play with.</param>
        /// <param name="timing"><see cref="ITiming"/> object raising timing events.</param>
        public GameController(Size mapSize, ITiming timing) :
            this(mapSize)
        {
            Timing = timing;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="GameController"/> with given map size and players.
        /// </summary>
        /// <param name="mapSize">Size of the map to play with.</param>
        /// <param name="crossPlayer">A player playing with cross.</param>
        /// <param name="noughtPlayer">A player playing with nought.</param>
        public GameController(Size mapSize, Player crossPlayer, Player noughtPlayer) :
            this(mapSize)
        {
            AddPlayers(crossPlayer, noughtPlayer);
        }

        /// <summary>
        /// Adds players to game state.
        /// </summary>
        /// <param name="crossPlayer">A player playing with cross.</param>
        /// <param name="noughtPlayer">A player playing with nought.</param>
        /// <exception cref="InvalidOperationException">One marks was already subscribed.</exception>
        public void AddPlayers(Player crossPlayer, Player noughtPlayer)
        {
            State.AddPlayer(crossPlayer, Mark.Cross);
            State.AddPlayer(noughtPlayer, Mark.Nought);

            crossPlayer.MoveRequest += HandlePlayersMoveRequest;
            noughtPlayer.MoveRequest += HandlePlayersMoveRequest;
        }

        /// <summary>
        /// Gets a way of coordinates changes when moving along given direction.
        /// </summary>
        /// <param name="direction">Direction to retrieve.</param>
        /// <returns>Vector representing the given direction.</returns>
        /// <exception cref="InvalidEnumArgumentException">The direction parameter value is invalid.</exception>
        public static Point GetCoordsChangesAlongDirection(WinningCombinationDirection direction)
        {
            switch (direction)
            {
                case WinningCombinationDirection.Horizontal:
                    return new Point(1, 0);
                case WinningCombinationDirection.Diagonal:
                    return new Point(1, 1);
                case WinningCombinationDirection.Vertical:
                    return new Point(0, 1);
                case WinningCombinationDirection.AntiDiagonal:
                    return new Point(-1, 1);
                default:
                    throw new InvalidEnumArgumentException("direction", (int)direction,
                        typeof(WinningCombinationDirection));
            }
        }

        /// <summary>
        /// Handles <see cref="Player.MoveRequest"/> event of the players.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">A <see cref="PlayerMoveEventArgs"/> containing data about event.</param>
        public virtual void HandlePlayersMoveRequest(object sender, PlayerMoveEventArgs e)
        {
            MarkPointIfPossible(e.MyMove, State.GetMarkOfPlayer((Player)sender));
        }

        /// <summary>
        /// Tries to mark a specified point with specified mark and checks winning conditions.
        /// </summary>
        /// <param name="markPoint">Point to mark.</param>
        /// <param name="mark">Mark to put.</param>
        /// <returns>Returns true if point has been successfully marked, false otherwise.</returns>
        protected bool MarkPointIfPossible(Point markPoint, Mark mark)
        {
            SyncConext.Send((o) =>
            {
                if (mark == State.Current)
                {
                    if (State.TicTacToeMap.MarkPoint(markPoint, mark))
                    {
                        WinConditionMetEventArgs arg;
                        if ((arg = GetWinConditionMetEventArgs(markPoint, mark)) != null)
                        {
                            timing?.Stop();
                            WinConditionMet?.Invoke(this, arg);
                        }
                        else
                        {
                            State.ChangeCurrentMark();
                            timing?.Restart();
                            State.GetPlayerFromMark(State.Current).MakeMove();
                        }
                    }
                }
            }, this);
            try
            {
                return State.TicTacToeMap.GetMarkOnPoint(markPoint) == mark;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if one of the players won after marking a new field.
        /// </summary>
        /// <param name="point">Coordinates of a field that was marked.</param>
        /// <param name="mark">The lastly made mark.</param>
        /// <returns>If one of players won, returns WinConditionMetEventArgs with WinConditionMet event data.
        /// Otherwise returns null.</returns>
        private WinConditionMetEventArgs GetWinConditionMetEventArgs(Point point, Mark mark)
        {
            WinningCombinationDirection[] possibleDirs =
            {
                WinningCombinationDirection.Horizontal,
                WinningCombinationDirection.Diagonal,
                WinningCombinationDirection.Vertical,
                WinningCombinationDirection.AntiDiagonal
            };

            Map myMap = State.TicTacToeMap;

            foreach (WinningCombinationDirection dir in possibleDirs)
            {
                Point dxdy = GetCoordsChangesAlongDirection(dir);

                Point combinationStart = new Point();
                for (Point beingChecked = point;
                    myMap.IsPointMarked(beingChecked) && myMap.GetMarkOnPoint(beingChecked) == mark;
                    beingChecked = new Point(beingChecked.X - dxdy.X, beingChecked.Y - dxdy.Y))
                {
                    combinationStart = beingChecked;
                }

                bool sequenceNotFull = false;
                for (int i = 1; i < WinningCombinationCount; ++i)
                {
                    Point beingChecked = new Point(combinationStart.X + i * dxdy.X,
                        combinationStart.Y + i * dxdy.Y);
                    if (!myMap.IsPointMarked(beingChecked) || myMap.GetMarkOnPoint(beingChecked) != mark)
                    {
                        sequenceNotFull = true;
                        break;
                    }
                }

                if (!sequenceNotFull)
                {
                    return new WinConditionMetEventArgs(mark, combinationStart, dir);
                }
            }

            return null;
        }

        /// <summary>
        /// Handles <see cref="ITiming.TimePassed"/> event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">An EventArgs containing event data.</param>
        private void Timing_TimePassed(object sender, EventArgs e)
        {
            SyncConext.Post((o) =>
            {
                State.ChangeCurrentMark();
                State.GetPlayerFromMark(State.Current).MakeMove();
            }, this);
            
        }

        /// <summary>
        /// Gets <see cref="GameState"/> instance with information about the game.
        /// </summary>
        public GameState State
        {
            get;
        }

        /// <summary>
        /// Gets or sets <see cref="ITiming"/> object controlling time for player's move.
        /// </summary>
        public ITiming Timing
        {
            get
            {
                return timing;
            }

            set
            {
                timing = value;
                if (timing != null)
                {
                    timing.TimePassed += Timing_TimePassed;
                }
            }
        }

        /// <summary>
        /// Gets or sets <see cref="SynchronizationContext"/> used by this instance.
        /// </summary>
        public SynchronizationContext SyncConext
        {
            get; set;
        }

        /// <summary>
        /// Gets number of consecutive marks that ensure victory.
        /// </summary>
        public int WinningCombinationCount
        {
            get
            {
                return 5;
            }
        }
    }

    /// <summary>
    /// Provides data for <see cref="GameController.WinConditionMet"/> event.
    /// </summary>
    public class WinConditionMetEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="WinConditionMetEventArgs"/>.
        /// </summary>
        /// <param name="mark">Mark that won.</param>
        /// <param name="startingPoint">Starting point of winning combination.</param>
        /// <param name="dir">Direction of winning combination.</param>
        public WinConditionMetEventArgs(Mark mark, Point startingPoint, WinningCombinationDirection dir)
        {
            WinningMark = mark;
            CombinationStartingPoint = startingPoint;
            Direction = dir;
        }

        /// <summary>
        /// Gets winning mark.
        /// </summary>
        public Mark WinningMark
        {
            get;
        }

        /// <summary>
        /// Gets starting point of winning combination.
        /// </summary>
        public Point CombinationStartingPoint
        {
            get;
        }

        /// <summary>
        /// Gets direction of winning combination.
        /// </summary>
        public WinningCombinationDirection Direction
        {
            get;
        }
    }

    /// <summary>
    /// Represents method for handling <see cref="GameController.WinConditionMet"/> event.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">A <see cref="WinConditionMetEventArgs"/> containing event data.</param>
    public delegate void WinConditionMetEventHandler(object sender, WinConditionMetEventArgs e);

    /// <summary>
    /// Specifies possible directions for winning combination.
    /// </summary>
    public enum WinningCombinationDirection
    {
        /// <summary>
        /// Horizontal combination.
        /// </summary>
        Horizontal,
        /// <summary>
        /// Diagonal from top-left to bottom-right combination.
        /// </summary>
        Diagonal,
        /// <summary>
        /// Vertical combination.
        /// </summary>
        Vertical,
        /// <summary>
        /// Diagonal from top-right to bottom-left combination.
        /// </summary>
        AntiDiagonal
    }
}
