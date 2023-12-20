using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WindowsFormsAppCamera
{
    internal class UdpList
    {
        public UdpList() { }
    }
    internal class UdpBroadcast
    {
        private const int       _broadcastPort = 9293;
        private const string    _broadcastAddress = "255.255.255.255";

        private UdpClient       _udpClient;
        private string          _ipAddress;
        private int             _lastIpOctet;

        public UdpBroadcast()
        {
            _udpClient = new UdpClient();
            _udpClient.EnableBroadcast = true;
            
            GetLocalIPAddr();
        }

        public void SendMessage(string msg)
        {
            msg = $"n.n.n.{_lastIpOctet}:{msg}";
            var sendBytes = Encoding.ASCII.GetBytes(msg);
            _udpClient.Send(sendBytes, sendBytes.Length, new IPEndPoint(IPAddress.Parse(_broadcastAddress), _broadcastPort));
        }

        private void GetLocalIPAddr()
        {
            string localIP = "";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    _ipAddress = localIP;

                    byte[] ipBytes = ip.GetAddressBytes();
                    _lastIpOctet = ipBytes[ipBytes.Length - 1];

                    break;
                }
            }
        }

    }
}
