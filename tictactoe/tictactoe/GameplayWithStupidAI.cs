using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using TicTacToe.Game.Controller;
using TicTacToe.Game.Model;
using TicTacToe.Game.View;
using System.Windows.Forms;

namespace TicTacToe
{
    /// <summary>
    /// Provides <see cref="AppControl"/> class for gameplay against AI.
    /// </summary>
    public class GameplayWithStupidAI : GameplayBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="GameplayWithStupidAI"/> with specified size of map.
        /// </summary>
        /// <param name="mapSize">Size of the map to play with.</param>
        public GameplayWithStupidAI(Size mapSize) :
            base(mapSize)
        {
            EasyAI stupidAI = new EasyAI(null);
            controller = new GameController(mapSize, new User(), stupidAI)
            {
                Timing = new HostTimer(10),
                SyncConext = SynchronizationContext.Current
            };
            stupidAI.MyGame = controller.State;
        }

        /// <summary>
        /// Disposes of resources held by this instance of <see cref="GameplayWithStupidAI"/>.
        /// </summary>
        public override void Dispose()
        {
            (controller.Timing as HostTimer).Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Handles <see cref="MapView.MapViewClick"/> event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">A <see cref="MapViewClickEventArgs"/> containing event data.</param>
        protected override void MapView_MapViewClick(object sender, MapViewClickEventArgs e)
        {
            User user = (User)controller.State.GetPlayerFromMark(Mark.Cross);
            user.MarkPoint(e.Location);
        }
    }
}
