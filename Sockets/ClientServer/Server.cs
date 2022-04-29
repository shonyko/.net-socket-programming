using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Sockets.ClientServer
{
    class Server {
        private Socket socket;
        private readonly string ipv4;
        private readonly int port;
        private readonly List<Socket> clients;

        public Server(string ipv4, int port)
        {
            this.ipv4 = ipv4;
            this.port = port;
            clients = new List<Socket>();
        }

        public void Listen()
        {

            var localEndPoint = new IPEndPoint(IPAddress.Parse(ipv4), port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
            socket.Listen(100);

            socket.BeginAccept(new AsyncCallback(OnClientConnect), socket);
        }

        public void Close()
        {
            foreach(Socket client in clients)
            {
                if (!client.Connected) continue;
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
        }

        private void OnClientConnect(IAsyncResult ar)
        {
            var clientSocket = socket.EndAccept(ar);
            Console.WriteLine("SERVER: Client connected!");
            clients.Add(clientSocket);
        }
    }
}
