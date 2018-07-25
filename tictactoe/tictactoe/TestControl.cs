using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Game.View;
using TicTacToe.Game.Model;

namespace TicTacToe
{
    class TestControl : AppControl
    {
        private MapView mapView;

        private int crossNought = 0;

        public override void Dispose()
        {
            mapView.Dispose();
        }

        public override void InitializeWindow()
        {
            mapView = new MapView
            {
                Size = new System.Drawing.Size(MainWindow.ClientSize.Width, 500),
                Location = new System.Drawing.Point(0, 0),
                MapSize = new System.Drawing.Size(20, 20),
                ViewLocation = new System.Drawing.Point(0, 0),
                Zoom = 1.0
            };
            mapView.MapViewClick += (sender, e) =>
            {
                Mark mark;
                if (crossNought == 0)
                    mark = Mark.Cross;
                else
                    mark = Mark.Nought;

                mapView.MarkPoint(e.Location, mark);

                crossNought = 1 - crossNought;
            };

            MainWindow.Controls.Add(mapView);
        }
    }
}
