using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsAppCamera
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
#if TRACE
            DateTime dt = DateTime.Now;
            string traceFile = $"DivGrind-{dt:MM-dd-H-mm}.trace";

            Trace.AutoFlush = true;

            Stream fileTrace = File.Create(traceFile,
                4,
                FileOptions.WriteThrough);

            TextWriterTraceListener textFileListener = new
               TextWriterTraceListener(fileTrace);

            textFileListener.TraceOutputOptions |= TraceOptions.DateTime;

            Trace.Listeners.Add(textFileListener);
#endif
            Trace.TraceInformation("Trace Started");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
