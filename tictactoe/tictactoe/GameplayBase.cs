using System;
using System.Collections.Generic;
using TicTacToe.Game.View;
using System.Drawing;
using TicTacToe.Game.Controller;
using TicTacToe.Game.Model;
using System.Windows.Forms;

namespace TicTacToe
{
    /// <summary>
    /// Provides base class for <see cref="AppControl"/> classes concerned with gameplay.
    /// </summary>
    public abstract class GameplayBase : AppControl
    {
        /// <summary>
        /// <see cref="MapView"/> control displaying view of the map.
        /// </summary>
        protected MapView mapView;
        /// <summary>
        /// <see cref="CenteringPanel"/> containing controls above <see cref="mapView"/>.
        /// </summary>
        protected CenteringPanel upperPanel;
        /// <summary>
        /// <see cref="Button"/> that drops down options menu.
        /// </summary>
        protected Button optionsBtn;

        /// <summary>
        /// <see cref="GameController"/> handling game logic.
        /// </summary>
        protected GameController controller;

        /// <summary>
        /// Initializes a new instance of <see cref="GameplayBase"/>.
        /// </summary>
        /// <param name="mapSize">Size of the map.</param>
        public GameplayBase(Size mapSize)
        {
            mapView = new MapView(mapSize);
        }

        /// <summary>
        /// Disposes of the resources used by this instance of <see cref="GameplayBase"/>.
        /// </summary>
        public override void Dispose()
        {
            mapView.Dispose();
            upperPanel.Dispose();
            optionsBtn.Dispose();
        }

        /// <summary>
        /// Initializes main window.
        /// </summary>
        public override void InitializeWindow()
        {
            Label lblCurrentInfo = new Label
            {
                Text = "Aktualny gracz: ",
                Name = CurrentInfo,
                AutoSize = true,
                Font = SystemFonts.CaptionFont,
                ForeColor = Color.Blue
            };
            MarkDisplay markDisplay = new MarkDisplay
            {
                Current = Mark.Cross,
                Size = new Size(40, 40),
                Name = CurrentDisplay
            };
            Label lblTimeLeftInfo = null;
            CountingDownLabel lblTimeLeft = null;
            if (controller.Timing != null)
            {
                lblTimeLeftInfo = new Label
                {
                    AutoSize = true,
                    Font = SystemFonts.CaptionFont,
                    ForeColor = Color.Blue,
                    Name = TimeLeftInfo,
                    Text = "Czas do wykonania ruchu:"
                };
                lblTimeLeft = new CountingDownLabel
                {
                    AutoSize = true,
                    Font = SystemFonts.CaptionFont,
                    ForeColor = Color.Blue,
                    Name = TimeLeft,
                    Time = TimeSpan.FromSeconds(controller.Timing.MoveTime)
                };
            }

            mapView.Location = new Point(0, 50);
            mapView.Size = new Size(MainWindow.ClientSize.Width, MainWindow.ClientSize.Height - 50);
            mapView.MapViewClick += MapView_MapViewClick;

            upperPanel = new CenteringPanel
            {
                Size = new Size(MainWindow.ClientSize.Width - 120, 50),
                Location = new Point(0, 0),
                DefaultControlsDistance = 10
            };

            upperPanel.SuspendLayout();
            upperPanel.AddToPanel(lblCurrentInfo);
            upperPanel.AddToPanel(markDisplay);
            if (lblTimeLeft != null)
            {
                upperPanel.AddToPanel(lblTimeLeftInfo, 20);
                upperPanel.AddToPanel(lblTimeLeft, 0);
            }
            upperPanel.ResumeLayout();

            optionsBtn = new Button
            {
                Text = "Opcje",
                Size = new Size(100, 35),
                BackColor = Color.Green,
                ForeColor = Color.White,
                Location = new Point(MainWindow.ClientSize.Width - 110, 7),
                FlatStyle = FlatStyle.Flat
            };
            optionsBtn.Click += OptionsButton_Click;

            MainWindow.Controls.Add(mapView);
            MainWindow.Controls.Add(upperPanel);
            MainWindow.Controls.Add(optionsBtn);

            controller.State.TicTacToeMap.MarksChanged += (sender, e) =>
            {
                MainWindow.BeginInvoke(new MarksChangedEventHandler(Map_MarksChanged), sender, e);
            };
            controller.WinConditionMet += (sender, e) =>
            {
                MainWindow.BeginInvoke(new WinConditionMetEventHandler(Controller_WinConditionMet), sender, e);
            };
            controller.State.CurrentChanged += (sender, e) =>
            {
                MainWindow.BeginInvoke(new EventHandler(State_CurrentChanged), sender, e);
            };

            if (controller.Timing != null)
            {
                controller.Timing.Tick += (sender, e) =>
                {
                    MainWindow.BeginInvoke(new EventHandler(Timing_Tick), sender, e);
                };
                controller.Timing.Start();
            }
        }

        /// <summary>
        /// When overriden in derived class handles <see cref="MapView.MapViewClick"/> event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">A <see cref="MapViewClickEventArgs"/> containing event data.</param>
        protected abstract void MapView_MapViewClick(object sender, MapViewClickEventArgs e);

        /// <summary>
        /// Handles <see cref="GameController.WinConditionMet"/> event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">A <see cref="WinConditionMetEventArgs"/> containing event data.</param>
        private void Controller_WinConditionMet(object sender, WinConditionMetEventArgs e)
        {
            controller.Timing?.Stop();
            WinningAnimation winningAnimation = new WinningAnimation(mapView, e)
            {
                Interval = 100,
                AnimationTime = 1500,
                CombinationLength = controller.WinningCombinationCount
            };

            mapView.MapViewClick -= MapView_MapViewClick;
            mapView.ShowLastMarkedTile = false;

            Point middleTile = new Point();
            switch (e.Direction)
            {
                case WinningCombinationDirection.Horizontal:
                    middleTile = new Point(e.CombinationStartingPoint.X + controller.WinningCombinationCount / 2,
                        e.CombinationStartingPoint.Y);
                    break;
                case WinningCombinationDirection.Diagonal:
                    middleTile = new Point(e.CombinationStartingPoint.X + controller.WinningCombinationCount / 2,
                        e.CombinationStartingPoint.Y + controller.WinningCombinationCount / 2);
                    break;
                case WinningCombinationDirection.Vertical:
                    middleTile = new Point(e.CombinationStartingPoint.X,
                        e.CombinationStartingPoint.Y + controller.WinningCombinationCount / 2);
                    break;
                case WinningCombinationDirection.AntiDiagonal:
                    middleTile = new Point(e.CombinationStartingPoint.X - controller.WinningCombinationCount / 2,
                        e.CombinationStartingPoint.Y + controller.WinningCombinationCount / 2);
                    break;
            }

            mapView.CenterOnTile(middleTile);

            winningAnimation.Start();
        }

        /// <summary>
        /// Handles <see cref="Map.MarksChanged"/> event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">A <see cref="MarksChangedEventArgs"/> containing event data.</param>
        private void Map_MarksChanged(object sender, MarksChangedEventArgs e)
        {
            mapView.MarkPoint(e.ChangedPoint, (sender as Map).GetMarkOnPoint(e.ChangedPoint));
        }

        /// <summary>
        /// Handles <see cref="Control.Click"/> event of the options button.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">An <see cref="EventArgs"/> containing event data.</param>
        private void OptionsButton_Click(object sender, EventArgs e)
        {
            Panel dropDown = optionsBtn.Tag as Panel;
            if (dropDown == null)
            {
                dropDown = new Panel
                {
                    Location = new Point(optionsBtn.Location.X - 50,
                        optionsBtn.Location.Y + optionsBtn.Height),
                    Size = new Size(150, 60),
                    BorderStyle = BorderStyle.FixedSingle
                };

                Button settings = new Button
                {
                    Location = new Point(0, 0),
                    Size = new Size(150, 30),
                    BackColor = Color.White,
                    ForeColor = Color.Black,
                    FlatStyle = FlatStyle.Flat,
                    Text = "Ustawienia"
                };
                settings.FlatAppearance.BorderSize = 0;
                settings.Click += (s, eventArgs) =>
                {
                    dropDown.Dispose();
                    throw new NotImplementedException();
                };
                Button quit = new Button
                {
                    Location = new Point(0, 30),
                    Size = new Size(150, 30),
                    BackColor = Color.White,
                    ForeColor = Color.Black,
                    FlatStyle = FlatStyle.Flat,
                    Text = "Wyjdź"
                };
                quit.FlatAppearance.BorderSize = 0;
                quit.Click += (s, eventArgs) =>
                {
                    dropDown.Dispose();
                    CtrlInstance = new PrimitiveGui();
                };
                dropDown.Controls.Add(settings);
                dropDown.Controls.Add(quit);

                MainWindow.Controls.Add(dropDown);
                dropDown.BringToFront();
                optionsBtn.Tag = dropDown;
            }
            else
            {
                dropDown.Dispose();
                optionsBtn.Tag = null;
            }
        }

        /// <summary>
        /// Handles <see cref="GameState.CurrentChanged"/> event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">An <see cref="EventArgs"/> containing event data.</param>
        private void State_CurrentChanged(object sender, EventArgs e)
        {
            MarkDisplay markDisplay = (MarkDisplay)upperPanel.Controls[CurrentDisplay];
            markDisplay.Current = controller.State.Current;

            if (controller.Timing != null)
            {
                CountingDownLabel counter = (CountingDownLabel)upperPanel.Controls[TimeLeft];
                counter.Time = TimeSpan.FromSeconds(controller.Timing.MoveTime);
            }
        }

        /// <summary>
        /// Handles <see cref="ITiming.Tick"/> event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">An <see cref="EventArgs"/> containing event data.</param>
        private void Timing_Tick(object sender, EventArgs e)
        {
            ITiming source = (ITiming)sender;

            CountingDownLabel counter = (CountingDownLabel)upperPanel.Controls[TimeLeft];
            counter.Time = TimeSpan.FromSeconds(source.TimeLeft);
        }
        
        private static readonly string CurrentDisplay = "markDisplay";
        
        private static readonly string CurrentInfo = "lblCurrentInfo";
        
        private static readonly string TimeLeftInfo = "lblTimeLeftInfo";
        
        private static readonly string TimeLeft = "lblTimeLeft";
    }
}
