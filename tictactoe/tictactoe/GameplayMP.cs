using System;
using System.Drawing;
using TicTacToe.Game.Model;
using System.ComponentModel;
using TicTacToe.Game.View;
using System.Windows.Forms;
using TicTacToe.Game.Controller;

namespace TicTacToe
{
    /// <summary>
    /// Provides base class of <see cref="AppControl"/> for multiplayer gameplay.
    /// </summary>
    public class GameplayMP : GameplayBase
    {
        private Mark userMark;
        protected Connection connection;

        /// <summary>
        /// Initializes a new instance of <see cref="GameplayMP"/> with specified size of map and mark of user.
        /// </summary>
        /// <param name="mapSize">Size of the map to play with.</param>
        /// <param name="userMark"><see cref="Mark"/> of the user in game.</param>
        public GameplayMP(Size mapSize, Mark userMark) :
            base(mapSize)
        {
            if (userMark != Mark.Cross && userMark != Mark.Nought)
                throw new InvalidEnumArgumentException("userMark", (int)userMark, typeof(Mark));
            this.userMark = userMark;
        }

        /// <summary>
        /// Initializes a main window for this <see cref="AppControl"/>.
        /// </summary>
        public override void InitializeWindow()
        {
            controller.State.TicTacToeMap.MarksChanged += Map_MarksChanged;

            NetPlayer remotePlayer =
                controller.State.GetPlayerFromMark(userMark == Mark.Cross ? Mark.Nought : Mark.Cross) as NetPlayer;
            controller.WinConditionMet += (sender, e) =>
            {
                remotePlayer.GameQuitted -= NetPlayer_GameQuitted;
            };

            remotePlayer.GameQuitted += NetPlayer_GameQuitted;
            MainWindow.FormClosing += Form_FormClosing;

            base.InitializeWindow();
        }

        /// <summary>
        /// Disposes of resources held by this instance of <see cref="GameplayMP"/>.
        /// </summary>
        public override void Dispose()
        {
            connection.SendInformation(NetPlayer.QuitCode, null);
            connection.Dispose();
            MainWindow.FormClosing -= Form_FormClosing;
            base.Dispose();
        }

        /// <summary>
        /// Handles <see cref="MapView.MapViewClick"/> event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">A <see cref="MapViewClickEventArgs"/> containing event data.</param>
        protected override void MapView_MapViewClick(object sender, MapViewClickEventArgs e)
        {
            User user = (User)controller.State.GetPlayerFromMark(userMark);
            user.MarkPoint(e.Location);
        }

        /// <summary>
        /// Handles <see cref="Form.FormClosing"/> event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">A <see cref="FormClosingEventArgs"/> containing event data.</param>
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            connection.SendInformation(NetPlayer.QuitCode, null);
        }

        /// <summary>
        /// Handles <see cref="Map.MarksChanged"/> event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">A <see cref="MarksChangedEventArgs"/> containing event data.</param>
        private void Map_MarksChanged(object sender, MarksChangedEventArgs e)
        {
            if ((sender as Map).GetMarkOnPoint(e.ChangedPoint) == userMark)
            {
                connection.SendInformation(NetPlayer.MarkCode, e);
            }
        }

        /// <summary>
        /// Handles <see cref="NetPlayer.GameQuitted"/> event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">An <see cref="EventArgs"/> containing event data.</param>
        private void NetPlayer_GameQuitted(object sender, EventArgs e)
        {
            MainWindow.BeginInvoke(new EventHandler((snd, args) =>
            {
                controller.Timing.Stop();
                MessageBox.Show("Przeciwnik opuścił grę", "Niestety...", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                mapView.MapViewClick -= MapView_MapViewClick;
            }));
        }
    }
}
