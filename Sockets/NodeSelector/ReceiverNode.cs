using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sockets.NodeSelector {
    class ReceiverNode {
        private Socket inSocket;
        private readonly IPEndPoint localEndPoint;

        private Socket outSocket;
        private readonly IPEndPoint remoteEndPoint;

        private readonly Func<int, bool> payloadValidator;

        public ReceiverNode(string localIpAddress, int localPort, string remoteIpAddress, int remotePort, Func<int, bool> payloadValidator) {
            localEndPoint = new IPEndPoint(IPAddress.Parse(localIpAddress), localPort);
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIpAddress), remotePort);
            this.payloadValidator = payloadValidator;

            outSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            outSocket.Bind(new IPEndPoint(localEndPoint.Address, 0));

            inSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            inSocket.Bind(localEndPoint);
        }

        public void StartListening() {
            byte[] buffer = new byte[256];
            var args = new SocketAsyncEventArgs() {
                RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0),
            };
            args.SetBuffer(buffer, 0, 256);
            args.Completed += OnReceive;

            var pending = inSocket.ReceiveFromAsync(args);
            if (!pending) OnReceive(inSocket, args);
        }

        public void OnReceive(object sender, SocketAsyncEventArgs args) {
            var paylaod = Encoding.ASCII.GetString(args.Buffer);
            var value = int.Parse(paylaod);
            
            if (payloadValidator(value)) {
                Console.WriteLine($"{localEndPoint.Address}: Got {value}, sending ACK...");

                var buffer = Encoding.ASCII.GetBytes("ACK");
                var new_args = new SocketAsyncEventArgs() {
                    RemoteEndPoint = remoteEndPoint
                };
                new_args.SetBuffer(buffer, 0, buffer.Length);
                outSocket.SendToAsync(new_args);
            } else {
                Console.WriteLine($"{localEndPoint.Address}: Got {value}");
            }

            var pending = inSocket.ReceiveFromAsync(args);
            if (!pending) OnReceive(inSocket, args);
        }
    }
}
