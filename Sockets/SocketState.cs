using System.Net.Sockets;

namespace Sockets
{
    class SocketState
    {
        // Client socket.  
        public Socket socket = null;
        // Size of receive buffer.  
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
    }
}
