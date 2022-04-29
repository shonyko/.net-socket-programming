using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Sockets.Relay
{
    class SenderNode
    {
        private string localipv4;
        private int localport;

        private Socket socket;
        private string nextipv4;
        private int nextport;

        private List<string> ips;

        public SenderNode(string localipv4, int localport, string nextipv4, int nextport, List<string> ips)
        {
            this.localipv4 = localipv4;
            this.localport = localport;
            this.nextipv4 = nextipv4;
            this.nextport = nextport;
            this.ips = ips;
        }

        public async Task Start()
        {
            var localEndPoint = new IPEndPoint(IPAddress.Parse(localipv4), 0);
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(nextipv4), nextport);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
            socket.Connect(remoteEndPoint);

            var rnd = new Random();
            for(int i = 0; i < 100; i++)
            {
                var index = rnd.Next(ips.Count);
                var payload = new Payload()
                {
                    targetIP = ips[index],
                    value = i
                };

                byte[] buffer;

                var bf = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, payload);
                    buffer = ms.ToArray();
                }

                await socket.SendAsync(buffer, SocketFlags.None);
                await Task.Delay(5);
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}
