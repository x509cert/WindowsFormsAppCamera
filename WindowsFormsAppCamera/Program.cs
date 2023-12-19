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
            var dt = DateTime.Now;
            var traceFile = $"DivGrind-{dt:MM-dd-H-mm}.trace";
            const int BUFF_SIZE = 1024;

            Trace.AutoFlush = true;

            Stream fileTrace = File.Create(traceFile,
                BUFF_SIZE,
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
