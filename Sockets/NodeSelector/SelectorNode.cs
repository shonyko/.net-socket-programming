using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sockets.NodeSelector
{
    class SelectorNode
    {
        private Socket inSocket;
        private string localipv4;
        private int localport;

        private Socket outSocket;
        private int remoteport;

        private List<string> ips;

        public SelectorNode(string localipv4, int localport, int remoteport, List<string> ips)
        {
            this.localipv4 = localipv4;
            this.localport = localport;
            this.remoteport = remoteport;
            this.ips = ips;
        }

        public async Task Start()
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


            outSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            outSocket.Bind(new IPEndPoint(IPAddress.Parse(localipv4), 0));
            var rnd = new Random();
            for (int i = 0; i < 100; i++)
            {
                var index = rnd.Next(ips.Count);

                byte[] new_buffer = Encoding.ASCII.GetBytes(i.ToString());
                var new_args = new SocketAsyncEventArgs()
                {
                    RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ips[index]), remoteport)
                };
                new_args.SetBuffer(new_buffer, 0, new_buffer.Length);
                outSocket.SendToAsync(new_args);

                await Task.Delay(5);
            }

            outSocket.Shutdown(SocketShutdown.Both);
            outSocket.Close();
        }

        public void OnReceive(object sender, SocketAsyncEventArgs args)
        {
            var paylaod = Encoding.ASCII.GetString(args.Buffer, 0, args.BytesTransferred);
            if (paylaod.Equals("ACK"))
            {
                Console.WriteLine($"{localipv4}: Got ACK from {args.RemoteEndPoint}");
            }

            var pending = inSocket.ReceiveFromAsync(args);
            if (!pending) OnReceive(inSocket, args);
        }
    }
}
