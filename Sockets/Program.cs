using Sockets.NodeSelector;
using Sockets.Relay;
using Sockets.Ring;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sockets.ClientServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Functioneaza!");

            // Client Server
            await ClientServer();

            // Ring
            //var ringNodes = new List<RingNode>()
            //{
            //    new RingNode("127.0.0.1", 1234, "127.0.0.2", 1234),
            //    new RingNode("127.0.0.2", 1234, "127.0.0.3", 1234),
            //    new RingNode("127.0.0.3", 1234, "127.0.0.1", 1234)
            //};

            //foreach(var rn in ringNodes)
            //{
            //    rn.Start();
            //}

            //ringNodes[0].Initiate();

            //while (true)
            //{
            //    var nb = -1;
            //    foreach (var rn in ringNodes)
            //    {
            //        nb = Math.Max(nb, rn.LastNumber);
            //    }
            //    if (nb == RingNode.MAX_NUMBER) break;
            //}

            // Node selector
            //var selector_ips = new List<string>()
            //{
            //    "127.0.0.2", "127.0.0.3"
            //};
            //var validators = new List<Func<int, bool>>()
            //{
            //    x => x % 3 == 0,
            //    x => x % 5 == 0
            //};
            //var selectorNode = new SelectorNode("127.0.0.1", 1234, 1234, selector_ips);
            //var receiverNodes = new List<ReceiverNode>();
            //foreach(var ip in selector_ips)
            //{
            //    var index = selector_ips.IndexOf(ip);
            //    receiverNodes.Add(new ReceiverNode(ip, 1234, "127.0.0.1", 1234, validators[index]));
            //}

            //foreach (var node in receiverNodes)
            //{
            //    node.Start();
            //}

            //await selectorNode.Start();

            // Relay
            //var relay_ips = new List<string>()
            //{
            //    "127.0.0.1", "127.0.0.2", "127.0.0.3"
            //};
            //var senderNode = new SenderNode("127.0.0.15", 1234, relay_ips[0], 1234, relay_ips);
            //var relayNodes = new List<RelayNode>();
            //for (int i = 0; i < relay_ips.Count - 1; i++)
            //{
            //    relayNodes.Add(new RelayNode(relay_ips[i], 1234, relay_ips[i + 1], 1234));
            //}
            //relayNodes.Add(new RelayNode(relay_ips[^1], 1234));

            //foreach (var node in relayNodes)
            //{
            //    node.Start();
            //}

            //await senderNode.Start();
        }

        static async Task ClientServer() {
            var client = new Client("127.0.0.1", 1234, "127.0.0.2", 1234);
            var server = new Server("127.0.0.2", 1234);

            server.StartListening();
            await client.Connect();
            client.Disconnect();
            server.StopListening();
        }
    }
}
