using System;
using System.Net;
using System.Net.Sockets;

namespace Sockets.ClientServer
{
    class Client
    {
        private Socket socket;
        private readonly string ipv4;
        private readonly int port;

        public Client(string ipv4, int port)
        {
            this.ipv4 = ipv4;
            this.port = port;
        }

        public void Connect()
        {
            var serverEndPoint = new IPEndPoint(IPAddress.Parse(ipv4), port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.BeginConnect(serverEndPoint, new AsyncCallback(OnConnect), socket);
        }

        public void Disconnect()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            Console.WriteLine("CLIENT: Disconnected from server!");
        }

        public bool Connected => socket.Connected;

        private void OnConnect(IAsyncResult ar)
        {
            socket.EndConnect(ar);
            Console.WriteLine("CLIENT: Connected to server!");
        }
    }
}
