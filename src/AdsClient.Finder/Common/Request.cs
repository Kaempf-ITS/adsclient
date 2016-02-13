using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ads.Client.Finder.Common
{
    public class Request
    {
        UdpClient client;
        public UdpClient Client { get { return client; } }
        public int Timeout;

        int udpPort = 48899;

        public Request(int timeout = 10000, int adsUdpPort = 48899)
        {
            Timeout = timeout;

            udpPort = adsUdpPort;

            client = new UdpClient();
            client.EnableBroadcast = true;
            client.Client.ReceiveTimeout = client.Client.SendTimeout = Timeout;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        public async Task<Response> SendAsync(IPEndPoint endPoint)
        {
            byte[] data = GetRequestBytes;
            await client.SendAsync(data, data.Length, endPoint);

            return new Response(client, Timeout);
        }

        List<byte[]> listOfBytes = new List<byte[]>();
        public byte[] GetRequestBytes
        {
            get { return listOfBytes.SelectMany(a => a).ToArray(); }
        }

        public void Add(byte[] segment)
        {
            listOfBytes.Add(segment);
        }

        public void Clear()
        {
            listOfBytes.Clear();
        }
    }
}
