using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sockets.Ring {
    class RingNode {
        public const int MAX_NUMBER = 100;

        private Socket inSocket;
        private readonly IPEndPoint localEndPoint;

        private Socket outSocket;
        private readonly IPEndPoint remoteEndPoint;

        public RingNode(string localIpAddress, int localPort, string remoteIpAddress, int remotePort) {
            localEndPoint = new IPEndPoint(IPAddress.Parse(localIpAddress), localPort);
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIpAddress), remotePort);

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

                var payload = Encoding.ASCII.GetString(state.buffer, 0, readBytes);
                var number = int.Parse(payload);
                Console.WriteLine($"{localEndPoint.Address}: Got number {number}");

                if (number < MAX_NUMBER) {
                    number++;
                    SendData(Encoding.ASCII.GetBytes(number.ToString()));
                } else Semaphore.Finished = true;

                state.socket.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0, new AsyncCallback(OnReceive), state);
            } catch(Exception e) {
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

        public void Initiate() {
            var payload = Encoding.ASCII.GetBytes("1");
            SendData(payload);
        }
    }
}
