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
        private int             _broadcastPort = 9293;
        private const string    _broadcastAddress = "255.255.255.255";

        private UdpClient       _udpClient;
        private string          _ipAddress;
        private int             _lastIpOctet;

        public UdpBroadcast(int port)
        {
            _udpClient = new UdpClient();
            _udpClient.EnableBroadcast = true;
            _broadcastPort = port;

            (_ipAddress, _lastIpOctet) = GetLocalIPAddr();
        }

        public void SendMessage(string msg)
        {
            msg = $"n.n.n.{_lastIpOctet}:{msg}";
            var sendBytes = Encoding.ASCII.GetBytes(msg);
            _udpClient.Send(sendBytes, 
                    sendBytes.Length, 
                    new IPEndPoint(IPAddress.Parse(_broadcastAddress), _broadcastPort));
        }

        static public (string, int) GetLocalIPAddr()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    var ipAddress = ip.ToString();

                    byte[] ipBytes = ip.GetAddressBytes();
                    var lastIpOctet = ipBytes[ipBytes.Length - 1];

                    return (ipAddress, lastIpOctet);
                }
            }

            return (null, 0);
        }
    }
}
