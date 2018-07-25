using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace TicTacToe
{
    /// <summary>
    /// Provides program's state information.
    /// </summary>
    public abstract class AppControl : IDisposable
    {
        private static AppControl ctrlInstance = null;
        private static MainForm mainWindow;

        /// <summary>
        /// When overriden in derived class disposes of components used by this instance.
        /// </summary>
        public virtual void Dispose()
        {
        }
        /// <summary>
        /// When overriden in derived class initializes window with new application state.
        /// </summary>
        public virtual void InitializeWindow()
        {
        }

        /// <summary>
        /// Gets or sets application's main window.
        /// </summary>
        public static MainForm MainWindow
        {
            get
            {
                return mainWindow;
            }
            set
            {
                mainWindow = value;
            }
        }

        /// <summary>
        /// Gets or sets current AppControl instance.
        /// </summary>
        public static AppControl CtrlInstance
        {
            get
            {
                return ctrlInstance;
            }
            set
            {
                if (ctrlInstance != null)
                    ctrlInstance.Dispose();
                ctrlInstance = value;
                if (ctrlInstance != null)
                    ctrlInstance.InitializeWindow();

                mainWindow.Invalidate();
            }
        }

        /// <summary>
        /// Beeping. See WinAPI for details.
        /// </summary>
        /// <param name="dwFreq">Frequency of sound, in hertz.</param>
        /// <param name="dwDuration">Duration of sound, in milliseconds.</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is 0.</returns>
        [DllImport("kernel32.dll")]
        public static extern int Beep(uint dwFreq, uint dwDuration);
    }
}
