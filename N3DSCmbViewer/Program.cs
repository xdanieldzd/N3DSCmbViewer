using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace N3DSCmbViewer
{
    static class Program
    {
        public static string Description = System.Reflection.Assembly.GetExecutingAssembly().GetAttribute<System.Reflection.AssemblyDescriptionAttribute>().Description;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
