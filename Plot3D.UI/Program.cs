using System;
using System.Windows.Forms;

namespace Plot3D.UI
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the Plot3D UI application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
#if NETCOREAPP || NET5_0_OR_GREATER
            ApplicationConfiguration.Initialize();
#else
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#endif
            Application.Run(new MainForm());
        }
    }
}
