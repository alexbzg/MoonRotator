using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EncRotator
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main( string[] args )
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new fMain( args ));
        }
    }
}
