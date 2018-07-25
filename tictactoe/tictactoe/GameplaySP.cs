using TicTacToe.Game.View;
using TicTacToe.Game.Controller;
using TicTacToe.Game.Model;
using System.Drawing;
using System.Threading;

namespace TicTacToe
{
    /// <summary>
    /// Provides <see cref="AppControl"/> class for single player gameplay.
    /// </summary>
    public class GameplaySP : GameplayBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="GameplaySP"/> with specified map size.
        /// </summary>
        /// <param name="mapSize">Size of the map to play with.</param>
        public GameplaySP(Size mapSize) :
            base(mapSize)
        {
            controller = new GameController(mapSize, new User(), new User())
            {
                Timing = new HostTimer(10),
                SyncConext = SynchronizationContext.Current
            };
        }

        /// <summary>
        /// Disposes of resources held by this instance of <see cref="GameplaySP"/>.
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
            User user = (User)controller.State.GetPlayerFromMark(controller.State.Current);
            user.MarkPoint(e.Location);
        }
        
    }
}
