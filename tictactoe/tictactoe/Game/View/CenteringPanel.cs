using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace TicTacToe.Game.View
{
    /// <summary>
    /// Container of controls alinging them along horizontal axis and centering them vertically.
    /// </summary>
    public class CenteringPanel : Panel
    {
        private IDictionary<Control, int> distances = new Dictionary<Control, int>();

        /// <summary>
        /// Adds specified control to the panel.
        /// </summary>
        /// <param name="control"><see cref="Control"/> to be added.</param>
        /// <exception cref="Exception">The specified control is a top-level control, or a circular control
        /// reference would result if this control were added to the control collection.</exception>
        /// <exception cref="ArgumentException">The object assigned to the value parameter
        /// is not a <see cref="Control"/>.</exception>
        /// <exception cref="InvalidOperationException">Control was already added.</exception>
        public void AddToPanel(Control control)
        {
            AddToPanel(control, DefaultControlsDistance);
        }

        /// <summary>
        /// Adds specified control to the panel and places it in specified distance from previous control.
        /// </summary>
        /// <param name="control"><see cref="Control"/> to be added.</param>
        /// <param name="distanceToPrevious">Distance from previous control in the panel, in pixels.</param>
        /// <exception cref="Exception">The specified control is a top-level control, or a circular control
        /// reference would result if this control were added to the control collection.</exception>
        /// <exception cref="ArgumentException">The object assigned to the value parameter
        /// is not a <see cref="Control"/>.</exception>
        /// <exception cref="InvalidOperationException">Control was already added.</exception>
        public void AddToPanel(Control control, int distanceToPrevious)
        {
            if (!distances.ContainsKey(control))
            {
                distances[control] = distanceToPrevious;
                Controls.Add(control);
            }
            else
            {
                throw new InvalidOperationException("Control was already added.");
            }
        }

        /// <summary>
        /// Changes distance to previous element for specified control.
        /// </summary>
        /// <param name="control">Control for which distance should be changed.</param>
        /// <param name="newDistance">New distance to previous element.</param>
        /// <exception cref="InvalidOperationException">Specified control does not belong to the panel.</exception>
        public void ChangeControlsDistanceToPrevious(Control control, int newDistance)
        {
            try
            {
                distances[control] = newDistance;
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException("Specified control does not belong to the panel");
            }
        }

        /// <summary>
        /// Changes distance to previous element for control with specified index.
        /// </summary>
        /// <param name="controlIndex">Zero-based index of control for which change will be applied.</param>
        /// <param name="newDistance">New distance to previous element.</param>
        /// <exception cref="ArgumentOutOfRangeException">Specified index was out of range.</exception>
        public void ChangeControlsDistanceToPrevious(int controlIndex, int newDistance)
        {
            distances[Controls[controlIndex]] = newDistance;
        }

        /// <summary>
        /// Raises <see cref="Control.ControlAdded"/> event. (Overrides <see cref="Control.OnControlAdded(ControlEventArgs)"/>)
        /// </summary>
        /// <param name="e">A <see cref="ControlEventArgs"/> containing event data.</param>
        protected override void OnControlAdded(ControlEventArgs e)
        {
            if (distances.Count != Controls.Count)
            {
                distances[e.Control] = DefaultControlsDistance;
            }

            base.OnControlAdded(e);
        }

        /// <summary>
        /// Raises <see cref="Control.ControlRemoved"/> event. (Overrides <see cref="Control.OnControlRemoved(ControlEventArgs)"/>)
        /// </summary>
        /// <param name="e">A <see cref="ControlEventArgs"/> containing event data.</param>
        protected override void OnControlRemoved(ControlEventArgs e)
        {
            distances.Remove(e.Control);
            base.OnControlRemoved(e);
        }

        /// <summary>
        /// Raises <see cref="Control.Layout"/> event. (Overrides <see cref="Control.OnLayout(LayoutEventArgs)"/>)
        /// </summary>
        /// <param name="e">A <see cref="LayoutEventArgs"/> containing event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            int xCurrent = 0;
            foreach (Control c in Controls)
            {
                xCurrent += distances[c];
                c.Location = new Point(xCurrent, (Height - c.Height) / 2);
                xCurrent += c.Width;
            }

            base.OnLayout(e);
        }

        /// <summary>
        /// Gets or sets default distance between controls in the panel.
        /// </summary>
        public int DefaultControlsDistance { get; set; }
    }
}
