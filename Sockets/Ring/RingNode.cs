using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sockets.Ring
{
    class RingNode
    {
        private Socket inSocket;
        private string localipv4;
        private int localport;

        private Socket outSocket;
        private string remoteipv4;
        private int remoteport;

        public const int MAX_NUMBER = 100;

        public int LastNumber { get; set; }

        public RingNode(string localipv4, int localport, string remoteipv4, int remoteport)
        {
            this.localipv4 = localipv4;
            this.localport = localport;
            this.remoteipv4 = remoteipv4;
            this.remoteport = remoteport;
        }

        public void Start()
        {
            var localEndPoint = new IPEndPoint(IPAddress.Parse(localipv4), localport);
            inSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            inSocket.Bind(localEndPoint);
            inSocket.Listen(1);

            inSocket.BeginAccept(new AsyncCallback(OnClientConnected), inSocket);
        }

        private void OnClientConnected(IAsyncResult ar)
        {
            var socket = inSocket.EndAccept(ar);
            var state = new SocketState
            {
                socket = socket
            };
            socket.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0, new AsyncCallback(OnReceive), state);
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                var state = (SocketState)ar.AsyncState;
                var bytesRead = state.socket.EndReceive(ar);

                if (bytesRead == 0)
                {
                    state.socket.Shutdown(SocketShutdown.Both);
                    state.socket.Close();
                    inSocket.BeginAccept(new AsyncCallback(OnClientConnected), inSocket);
                    return;
                }

                var content = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                var number = int.Parse(content);
                Console.WriteLine($"{localipv4}: Got number {number}");

                if (number < MAX_NUMBER)
                {
                    number++;
                    var payload = Encoding.ASCII.GetBytes(number.ToString());
                    var remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteipv4), remoteport);
                    var localEndPoint = new IPEndPoint(IPAddress.Parse(localipv4), 0);
                    outSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    outSocket.Bind(localEndPoint);
                    outSocket.Connect(remoteEndPoint);
                    outSocket.Send(payload);
                    outSocket.Shutdown(SocketShutdown.Both);
                    outSocket.Close();
                }
                LastNumber = number;

                state.socket.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0, new AsyncCallback(OnReceive), state);
            } catch(Exception e)
            {
                Console.WriteLine($"{localipv4}: {e.Message}");
            }
            
        }

        public void Initiate()
        {
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteipv4), remoteport);
            outSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            outSocket.Connect(remoteEndPoint);
            var payload = Encoding.ASCII.GetBytes("1");
            outSocket.Send(payload);
            outSocket.Shutdown(SocketShutdown.Both);
            outSocket.Close();
        }
    }
}
