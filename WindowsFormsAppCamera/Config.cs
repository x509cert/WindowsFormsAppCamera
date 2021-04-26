using System.Text.Json;
using System.IO;

namespace WindowsFormsAppCamera
{
    public class Config
    {
        public string MachineName { get; set; }
        public string LogUri { get; set; }
        public int LastCalibratedR { get; set; }
        public int LastCalibratedG { get; set; }
        public int LastCalibratedB { get; set; }
        public float ThreshHold { get; set; }
        public bool UsingLiveScreen { get; set; }

        // camera and video mode are an index into the iterators returned by UsbCamera
        public int Camera { get; set; }
        public int VideoMode { get; set; }
        public string ComPort { get; set; }

        // sms data
        public string AzureConnection { get; set; }
        public string FromNumber { get; set; }
        public string ToNumber { get; set; }
    }

    public partial class Form1
    {
        private const string ConfigFileName = "DivGrind.config";

        public void WriteConfig(Config cfg)
        {
            var options = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };
            var cf = JsonSerializer.Serialize(cfg, options);
            File.WriteAllText(ConfigFileName, cf);
        }

        public Config ReadConfig()
        {
            var cf = File.ReadAllText(ConfigFileName);
            cf = cf.Replace("\n", "").Replace("\r", "");
            return JsonSerializer.Deserialize<Config>(cf);
        }
    }
}
