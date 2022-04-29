using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sockets.NodeSelector
{
    class ReceiverNode
    {
        private Socket inSocket;
        private string localipv4;
        private int localport;

        private Socket outSocket;
        private string remoteipv4;
        private int remoteport;

        Func<int, bool> payloadValidator;

        public ReceiverNode(string localipv4, int localport, string remoteipv4, int remoteport, Func<int, bool> payloadValidator)
        {
            this.localipv4 = localipv4;
            this.localport = localport;
            this.remoteipv4 = remoteipv4;
            this.remoteport = remoteport;
            this.payloadValidator = payloadValidator;
        }

        public void Start()
        {
            var localEndPoint = new IPEndPoint(IPAddress.Parse(localipv4), localport);
            inSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            inSocket.Bind(localEndPoint);

            byte[] buffer = new byte[256];
            var args = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0),
            };
            args.SetBuffer(buffer, 0, 256);
            args.Completed += OnReceive;
            var pending = inSocket.ReceiveFromAsync(args);
            if (!pending) OnReceive(inSocket, args);
        }

        public void OnReceive(object sender, SocketAsyncEventArgs args)
        {
            var paylaod = Encoding.ASCII.GetString(args.Buffer);
            var value = int.Parse(paylaod);
            
            if (payloadValidator(value))
            {
                Console.WriteLine($"{localipv4}: Got {value}, sending ACK...");
                var localEndPoint = new IPEndPoint(IPAddress.Parse(localipv4), 0);
                outSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                outSocket.Bind(localEndPoint);

                var buffer = Encoding.ASCII.GetBytes("ACK");
                var new_args = new SocketAsyncEventArgs()
                {
                    RemoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteipv4), remoteport)
                };
                new_args.SetBuffer(buffer, 0, buffer.Length);
                outSocket.SendToAsync(new_args);
            } else
            {
                Console.WriteLine($"{localipv4}: Got {value}");
            }

            var pending = inSocket.ReceiveFromAsync(args);
            if (!pending) OnReceive(inSocket, args);
        }
    }
}
