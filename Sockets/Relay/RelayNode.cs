using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Sockets.Relay
{
    class RelayNode
    {
        private Socket inSocket;
        private string localipv4;
        private int localport;

        private bool hasNextHop;
        private Socket outSocket;
        private string nextipv4;
        private int nextport;

        public RelayNode(string localipv4, int localport, string nextipv4, int nextport)
        {
            this.localipv4 = localipv4;
            this.localport = localport;
            this.nextipv4 = nextipv4;
            this.nextport = nextport;
            hasNextHop = true;
        }

        public RelayNode(string localipv4, int localport)
        {
            this.localipv4 = localipv4;
            this.localport = localport;
            hasNextHop = false;
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

                Payload payload;
                var bf = new BinaryFormatter();
                using (var ms = new MemoryStream(state.buffer))
                {
                    object obj = bf.Deserialize(ms);
                    payload = (Payload)obj;
                }

                if(localipv4.Equals(payload.targetIP))
                {
                    Console.WriteLine($"{localipv4}: got number {payload.value}.");
                }
                else if (hasNextHop)
                {
                    var localEndPoint = new IPEndPoint(IPAddress.Parse(localipv4), 0);
                    var remoteEndPoint = new IPEndPoint(IPAddress.Parse(nextipv4), nextport);
                    outSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    outSocket.Bind(localEndPoint);
                    outSocket.Connect(remoteEndPoint);
                    outSocket.Send(state.buffer);
                    outSocket.Shutdown(SocketShutdown.Both);
                    outSocket.Close();
                }

                state.socket.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0, new AsyncCallback(OnReceive), state);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{localipv4}: {e.Message}");
            }

        }
    }
}
