using System;
using System.Net;
using System.Net.Sockets;

namespace Sockets.ClientServer {
    class Server {
        private Socket socket;
        private readonly IPEndPoint localEndPoint;

        public Server(string localIpAddress, int localPort) {
            localEndPoint = new IPEndPoint(IPAddress.Parse(localIpAddress), localPort);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
            socket.Listen(100);
        }

        public void StartListening() {
            socket.BeginAccept(new AsyncCallback(OnClientConnect), socket);
        }

        public void StopListening() {
            socket.Close();
        }

        private void OnClientConnect(IAsyncResult ar) {
            var clientSocket = socket.EndAccept(ar);
            var state = new SocketState {
                socket = clientSocket
            };
            clientSocket.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0, new AsyncCallback(OnReceive), state);
            Console.WriteLine("SERVER: Client connected!");

            socket.BeginAccept(new AsyncCallback(OnClientConnect), socket);
        }

        private void OnReceive(IAsyncResult ar) {
            var state = (SocketState)ar.AsyncState;
            var readBytes = state.socket.EndReceive(ar);

            if(readBytes <= 0) {
                state.socket.Shutdown(SocketShutdown.Both);
                state.socket.Close();
                Console.WriteLine("SERVER: Client disconnected!");
                return;
            }

            state.socket.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0, new AsyncCallback(OnReceive), state);
        }
    }
}
