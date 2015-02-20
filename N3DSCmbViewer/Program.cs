using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;

namespace N3DSCmbViewer
{
    static class Program
    {
        public static string Description = string.Format("{0} v{1}",
            Assembly.GetExecutingAssembly().GetAttribute<AssemblyDescriptionAttribute>().Description,
            new Version(Assembly.GetExecutingAssembly().GetAttribute<AssemblyFileVersionAttribute>().Version).ToString(3));

        [STAThread]
        static void Main()
        {
            /* Make sure OpenTK doesn't swallow ANY BLOODY EXCEPTION THAT OCCURES DURING RENDERING */
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
