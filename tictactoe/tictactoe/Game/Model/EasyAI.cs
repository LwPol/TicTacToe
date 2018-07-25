using System;
using System.Drawing;

namespace TicTacToe.Game.Model
{
    /// <summary>
    /// Represents AI making pretty random moves.
    /// </summary>
    public class EasyAI : Player
    {
        private Point myLastPoint = new Point(0, 0);
        private Random rng = new Random();

        /// <summary>
        /// Initializes a new instance of <see cref="EasyAI"/> class.
        /// </summary>
        /// <param name="state"><see cref="GameState"/> object representing game in which AI will be playing.</param>
        public EasyAI(GameState state)
        {
            MyGame = state;
        }

        /// <summary>
        /// Forces AI to make a move.
        /// </summary>
        public override void MakeMove()
        {
            Map myMap = MyGame.TicTacToeMap;

            Point randomPoint;
            int size = 2;
            do
            {
                int dx = rng.Next(-size, size);
                int dy = rng.Next(-size, size);
                randomPoint = new Point(myLastPoint.X + dx, myLastPoint.Y + dy);
                if (size < myMap.MapSize.Width && size < myMap.MapSize.Height)
                {
                    ++size;
                }
            } while (!myMap.IsPointOnMap(randomPoint) || myMap.IsPointMarked(randomPoint));

            myLastPoint = randomPoint;
            OnMoveRequest(new PlayerMoveEventArgs(randomPoint));
        }

        /// <summary>
        /// Gets or sets GameState used by this AI.
        /// </summary>
        public GameState MyGame
        {
            get; set;
        }
    }
}
