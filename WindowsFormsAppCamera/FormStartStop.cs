using System;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;

namespace WindowsFormsAppCamera
{
    public partial class Form1 : Form
    {
        // The main entry point
        public Form1()
        {
            InitializeComponent();

            // logs go in user's profile folder (eg; c:\users\mikehow)
            _sLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // get the date this binary was last modified
            string strpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FileInfo fi = new FileInfo(strpath);
            string buildDate = fi.LastWriteTime.ToString("yyMMdd:HHmm");

            // machine name
            string machine = Dns.GetHostName();

            // add info to title of the tools
            Text = $"DivGrind [{buildDate}] on {machine}";

            numTrigger.Value = (decimal)_triggerPercent;

            _logQueue = new LogQueue(50);

            _machineName = Dns.GetHostName();

            string machineName = "";
            string azureConnection = "";
            string azureSmsFrom = "";
            string azureSmsTo = "";
            string logUri = "";

            bool ok = GetCmdLineArgs(ref machineName, ref azureConnection, ref azureSmsFrom, ref azureSmsTo, ref logUri);
            if (ok)
            {
                // set machine name
                if (string.IsNullOrEmpty(machineName) == false) _machineName = machineName;

                // if the three args are available for SMS, then create an SmsAlert object
                if (string.IsNullOrEmpty(azureConnection) == false &&
                    string.IsNullOrEmpty(azureSmsFrom) == false &&
                    string.IsNullOrEmpty(azureSmsTo) == false)
                {
                    _smsAlert = new SmsAlert(machineName, azureConnection, azureSmsFrom, azureSmsTo)
                    {
                        BlockLateNightSms = true
                    };

                    txtSmsEnabled.Text = "Yes";
                    btnTestSms.Enabled = true;
                }

                if (string.IsNullOrEmpty(logUri) == false)
                    _logUri = logUri;
            }
        }

        // Code to read command-line args
        // -n "machinename" -c "connectionstring" -f "from sms #"  -t "to sms #" -u "log app URI"
        bool GetCmdLineArgs(ref string machineName,         // -n
                            ref string azureCommsString,    // -c
                            ref string azureSmsFrom,        // -f
                            ref string azureSmsTo,          // -t
                            ref string logUri)              // -u 
        {
            bool success = true;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 0) return false;

            try
            {
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i].ToLower().StartsWith("-n") == true)
                        txtName.Text = machineName = args[i + 1];

                    if (args[i].ToLower().StartsWith("-c") == true)
                        azureCommsString = args[i + 1];

                    if (args[i].ToLower().StartsWith("-f") == true)
                        azureSmsFrom = args[i + 1];

                    if (args[i].ToLower().StartsWith("-t") == true)
                        azureSmsTo = args[i + 1];

                    if (args[i].ToLower().StartsWith("-u") == true)
                        logUri = args[i + 1];
                }
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        // this gives the code a chance to kill the main worker thread gracefully
        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            KillSkillTimer();
            _fKillThreads = true;

            Thread.Sleep(400);
            e.Cancel = false;

            if (_sComPort != null)
            {
                _sComPort.Close();
                _sComPort = null;
            }
        }
    }
}


