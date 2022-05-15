using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Sockets.Relay {
    class RelayNode {
        private Socket inSocket;
        private readonly IPEndPoint localEndPoint;

        private Socket outSocket;
        private readonly IPEndPoint remoteEndPoint;

        private readonly bool hasNextHop = true;

        public RelayNode(string localIpAddress, int localPort, string remoteIpAddress, int remotePort) {
            localEndPoint = new IPEndPoint(IPAddress.Parse(localIpAddress), localPort);
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIpAddress), remotePort);

            inSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            inSocket.Bind(localEndPoint);
            inSocket.Listen(1);
        }

        public RelayNode(string localIpAddress, int localPort) {
            localEndPoint = new IPEndPoint(IPAddress.Parse(localIpAddress), localPort);
            hasNextHop = false;

            inSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            inSocket.Bind(localEndPoint);
            inSocket.Listen(1);
        }

        public void StartListening() {
            inSocket.BeginAccept(new AsyncCallback(OnClientConnected), null);
        }

        private void OnClientConnected(IAsyncResult ar) {
            var socket = inSocket.EndAccept(ar);
            var state = new SocketState {
                socket = socket
            };
            socket.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0, new AsyncCallback(OnReceive), state);
        }

        private void OnReceive(IAsyncResult ar) {
            try {
                var state = (SocketState)ar.AsyncState;
                var readBytes = state.socket.EndReceive(ar);

                if (readBytes <= 0) {
                    state.socket.Shutdown(SocketShutdown.Both);
                    state.socket.Close();
                    StartListening();
                    return;
                }

                Payload payload;
                var bf = new BinaryFormatter();
                using (var ms = new MemoryStream(state.buffer)) {
                    object obj = bf.Deserialize(ms);
                    payload = (Payload)obj;
                }

                if (localEndPoint.Address.ToString().Equals(payload.targetIP)) {
                    Console.WriteLine($"{localEndPoint.Address}: got number {payload.value}.");
                } else if (hasNextHop) {
                    Console.WriteLine($"{localEndPoint.Address}: forwarding to {payload.targetIP}.");
                    SendData(state.buffer);
                }

                state.socket.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0, new AsyncCallback(OnReceive), state);
            }
            catch (Exception e) {
                Console.WriteLine($"{localEndPoint.Address}: {e.Message}");
            }

        }

        private void SendData(byte[] data) {
            outSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            outSocket.Bind(new IPEndPoint(localEndPoint.Address, 0));
            outSocket.Connect(remoteEndPoint);

            outSocket.Send(data);

            outSocket.Shutdown(SocketShutdown.Both);
            outSocket.Close();
        }
    }
}
