using System;
using System.Collections.Generic;
using System.Linq;

namespace TicTacToe.Game.Model
{
    /// <summary>
    /// Represents state of tic-tac-toe game.
    /// </summary>
    public class GameState
    {
        private Mark current;

        private Dictionary<Mark, Player> playersAndMarks = new Dictionary<Mark, Player>();

        /// <summary>
        /// Occurs when Current property has changed.
        /// </summary>
        public event EventHandler CurrentChanged;
        /// <summary>
        /// Occurs when binding between marks and players has changed.
        /// </summary>
        public event MarkToPlayerBindingChangedEventHandler MarkToPlayerBindingChanged;

        /// <summary>
        /// Subscribes a <see cref="Player"/> to specified <see cref="Mark"/>.
        /// </summary>
        /// <param name="player"><see cref="Player"/> to subscribe.</param>
        /// <param name="playersMark"><see cref="Mark"/> to subsribe to.</param>
        /// <exception cref="InvalidOperationException">Player with specified mark already exists.</exception>
        public void AddPlayer(Player player, Mark playersMark)
        {
            lock (this)
            {
                if (playersAndMarks.ContainsKey(playersMark))
                {
                    throw new InvalidOperationException("Player with specified mark already exists");
                }

                playersAndMarks[playersMark] = player;
            }

            MarkToPlayerBindingChanged?.Invoke(this,
                new MarkToPlayerBindingChangedEventArgs(playersMark, player, MarkToPlayerBindingChangeType.PlayerAdded));
        }

        /// <summary>
        /// Changes current mark from cross to nought or from nought to cross.
        /// </summary>
        public void ChangeCurrentMark()
        {
            lock (this)
            {
                if (Current == Mark.Cross)
                {
                    current = Mark.Nought;
                }
                else
                {
                    current = Mark.Cross;
                }
            }

            CurrentChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets a mark used by the player.
        /// </summary>
        /// <param name="player"><see cref="Player"/> for which mark needs to be retrieved.</param>
        /// <returns>Mark used by the player.</returns>
        /// <exception cref="ArgumentNullException">The player parameter is null.</exception>
        /// <exception cref="ArgumentException">Specified Player is not subscribed to any mark.</exception>
        public Mark GetMarkOfPlayer(Player player)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            lock (this)
            {
                return playersAndMarks.Where((x) =>
                {
                    return ReferenceEquals(x.Value, player);
                }).First().Key;
            }
        }

        /// <summary>
        /// Gets <see cref="Player"/> subscribed to specified <see cref="Mark"/>.
        /// </summary>
        /// <param name="mark"><see cref="Mark"/> to retrieve the <see cref="Player"/> for.</param>
        /// <returns><see cref="Player"/> subscribed to specified mark.</returns>
        /// <exception cref="InvalidOperationException">There is no Player subscribed to specified mark.</exception>
        public Player GetPlayerFromMark(Mark mark)
        {
            try
            {
                lock (this)
                {
                    return playersAndMarks[mark];
                }
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException("Specified mark does not correspond to any of the players");
            }
        }

        /// <summary>
        /// Unsubscribes Player from specified mark.
        /// </summary>
        /// <param name="mark">Mark to unsubscribe from.</param>
        public void RemovePlayer(Mark mark)
        {
            Player player;
            lock (this)
            {
                if (playersAndMarks.TryGetValue(mark, out player))
                {
                    playersAndMarks.Remove(mark);
                }
            }

            if (player != null)
            {
                MarkToPlayerBindingChanged?.Invoke(this,
                    new MarkToPlayerBindingChangedEventArgs(mark, player, MarkToPlayerBindingChangeType.PlayerRemoved));
            }
        }

        /// <summary>
        /// Gets or sets mark that has currently right to move.
        /// </summary>
        public Mark Current
        {
            get
            {
                return current;
            }

            set
            {
                current = value;
                CurrentChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets TicTacToeMap object referenced by this GameState instance.
        /// </summary>
        public Map TicTacToeMap
        {
            get; set;
        }
    }

    /// <summary>
    /// Provides data for <see cref="GameState.MarkToPlayerBindingChanged"/> event.
    /// </summary>
    public class MarkToPlayerBindingChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MarkToPlayerBindingChangedEventArgs"/> class.
        /// </summary>
        /// <param name="mark">Mark that has been involved in the change.</param>
        /// <param name="player">Player that has been involved in the change.</param>
        /// <param name="changeType">Type of change that has occurred.</param>
        public MarkToPlayerBindingChangedEventArgs(Mark mark, Player player, MarkToPlayerBindingChangeType changeType)
        {
            Mark = mark;
            Player = player;
            ChangeType = changeType;
        }

        /// <summary>
        /// Gets Mark involved in the change.
        /// </summary>
        public Mark Mark
        {
            get;
        }

        /// <summary>
        /// Gets Player involved in the change.
        /// </summary>
        public Player Player
        {
            get;
        }

        /// <summary>
        /// Gets type of the change.
        /// </summary>
        public MarkToPlayerBindingChangeType ChangeType
        {
            get;
        }
    }

    /// <summary>
    /// Represents method for handling <see cref="GameState.MarkToPlayerBindingChanged"/> event.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">A <see cref="MarkToPlayerBindingChangedEventArgs"/> containing event data.</param>
    public delegate void MarkToPlayerBindingChangedEventHandler(object sender, MarkToPlayerBindingChangedEventArgs e);

    /// <summary>
    /// Specifies different types of changes of binding from mark to player.
    /// </summary>
    public enum MarkToPlayerBindingChangeType
    {
        /// <summary>
        /// A player was subscribed to a mark.
        /// </summary>
        PlayerAdded,
        /// <summary>
        /// A player was unsubscribed from a mark.
        /// </summary>
        PlayerRemoved
    }
}
