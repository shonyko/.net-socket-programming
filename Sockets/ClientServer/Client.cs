using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Sockets.ClientServer {
    class Client {
        private Socket socket;

        private readonly IPEndPoint localEndPoint;
        private readonly IPEndPoint remoteEndPoint;

        public Client(string localIpAddress, int localPort, string remoteIpAddress, int remotePort) {
            localEndPoint = new IPEndPoint(IPAddress.Parse(localIpAddress), localPort);
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIpAddress), remotePort);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(localEndPoint.Address, 0));
        }

        public Task Connect() {
            socket.BeginConnect(remoteEndPoint, new AsyncCallback(OnConnect), socket);

            while (!socket.Connected) ;
            return Task.CompletedTask;
        }

        public void Disconnect() {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            Console.WriteLine("CLIENT: Disconnected from server!");
        }

        private void OnConnect(IAsyncResult ar) {
            socket.EndConnect(ar);
            Console.WriteLine("CLIENT: Connected to server!");
        }
    }
}
