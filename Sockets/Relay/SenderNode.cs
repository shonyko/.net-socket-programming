using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Sockets.Relay {
    class SenderNode {
        public const int MAX_NUMBER = 100;

        private Socket socket;
        private readonly IPEndPoint localEndPoint;
        private readonly IPEndPoint remoteEndPoint;

        private List<string> remoteIps;

        public SenderNode(string localIpAddress, int localPort, string remoteIpAddress, int remotePort, List<string> ips) {
            localEndPoint = new IPEndPoint(IPAddress.Parse(localIpAddress), localPort);
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIpAddress), remotePort);
            this.remoteIps = ips;

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(localEndPoint.Address, 0));
        }

        public async Task Send() {
            socket.Connect(remoteEndPoint);

            var rnd = new Random();
            for(int i = 0; i < MAX_NUMBER; i++)
            {
                var index = rnd.Next(remoteIps.Count);
                var payload = new Payload() {
                    targetIP = remoteIps[index],
                    value = i
                };

                byte[] buffer;
                var bf = new BinaryFormatter();
                using (var ms = new MemoryStream()) {
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
