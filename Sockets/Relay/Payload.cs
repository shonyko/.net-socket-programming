using System;
using System.Net;

namespace Sockets.Relay
{
    [Serializable]
    class Payload
    {
        public string targetIP;
        public int value;
    }
}
