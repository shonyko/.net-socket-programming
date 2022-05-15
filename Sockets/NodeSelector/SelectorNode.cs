using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sockets.NodeSelector {
    class SelectorNode {
        public const int MAX_NUMBER = 100;

        private Socket inSocket;
        private readonly IPEndPoint localEndPoint;

        private Socket outSocket;
        private List<IPEndPoint> remoteEndPoints;


        public SelectorNode(string localIpAddress, int localPort, int remotePort, List<string> ips) {
            localEndPoint = new IPEndPoint(IPAddress.Parse(localIpAddress), localPort);
            remoteEndPoints = ips.Select(ip => new IPEndPoint(IPAddress.Parse(ip), remotePort)).ToList();

            outSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            outSocket.Bind(new IPEndPoint(localEndPoint.Address, 0));

            inSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            inSocket.Bind(localEndPoint);

            byte[] buffer = new byte[256];
            var args = new SocketAsyncEventArgs() {
                RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0),
            };
            args.SetBuffer(buffer, 0, 256);
            args.Completed += OnReceive;

            var pending = inSocket.ReceiveFromAsync(args);
            if (!pending) OnReceive(inSocket, args);
        }

        public async Task Send() {
            var rnd = new Random();
            for (int i = 0; i < MAX_NUMBER; i++) {
                var index = rnd.Next(remoteEndPoints.Count);

                byte[] buffer = Encoding.ASCII.GetBytes(i.ToString());
                var args = new SocketAsyncEventArgs() {
                    RemoteEndPoint = remoteEndPoints[index]
                };
                args.SetBuffer(buffer, 0, buffer.Length);
                outSocket.SendToAsync(args);

                await Task.Delay(5);
            }

            outSocket.Shutdown(SocketShutdown.Both);
            outSocket.Close();
        }

        public void OnReceive(object sender, SocketAsyncEventArgs args) {
            var paylaod = Encoding.ASCII.GetString(args.Buffer, 0, args.BytesTransferred);
            if (paylaod.Equals("ACK")) {
                Console.WriteLine($"{localEndPoint.Address}: Got ACK from {args.RemoteEndPoint}");
            }

            var pending = inSocket.ReceiveFromAsync(args);
            if (!pending) OnReceive(inSocket, args);
        }
    }
}
