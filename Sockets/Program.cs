using Sockets.NodeSelector;
using Sockets.Relay;
using Sockets.Ring;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sockets.ClientServer {
    class Program {
        static async Task Main(string[] args) {
            Console.WriteLine("Functioneaza!");

            // Client Server
            //await ClientServer();

            // Ring
            //await Ring();

            // Node selector
            //await NodeSelector();

            // Relay
            //await Relay();
        }

        static async Task ClientServer() {
            var client = new Client("127.0.0.1", 1234, "127.0.0.2", 1234);
            var server = new Server("127.0.0.2", 1234);

            server.StartListening();
            await client.Connect();
            client.Disconnect();
            server.StopListening();
        }

        static Task Ring() {
            var ringNodes = new List<RingNode>() {
                new RingNode("127.0.0.1", 1234, "127.0.0.2", 1234),
                new RingNode("127.0.0.2", 1234, "127.0.0.3", 1234),
                new RingNode("127.0.0.3", 1234, "127.0.0.1", 1234)
            };

            foreach (var rn in ringNodes) {
                rn.StartListening();
            }

            Semaphore.Finished = false;
            ringNodes[0].Initiate();
            while (!Semaphore.Finished) ;

            return Task.CompletedTask;
        }

        static async Task NodeSelector() {
            var selectorIps = new List<string>() {
                "127.0.0.2", "127.0.0.3"
            };
            var payloadValidators = new List<Func<int, bool>>() {
                x => x % 3 == 0,
                x => x % 5 == 0
            };
            var receiverNodes = new List<ReceiverNode>();
            foreach (var ip in selectorIps) {
                var index = selectorIps.IndexOf(ip);
                receiverNodes.Add(new ReceiverNode(ip, 1234, "127.0.0.1", 1234, payloadValidators[index]));
            }

            var selectorNode = new SelectorNode("127.0.0.1", 1234, 1234, selectorIps);

            foreach (var node in receiverNodes) {
                node.StartListening();
            }

            await selectorNode.Send();
        }

        static async Task Relay() {
            var relayIps = new List<string>() {
                "127.0.0.1", "127.0.0.2", "127.0.0.3"
            };
            var senderNode = new SenderNode("127.0.0.15", 1234, relayIps[0], 1234, relayIps);
            var relayNodes = new List<RelayNode>();
            for (int i = 0; i < relayIps.Count - 1; i++) {
                relayNodes.Add(new RelayNode(relayIps[i], 1234, relayIps[i + 1], 1234));
            }
            relayNodes.Add(new RelayNode(relayIps[^1], 1234));

            foreach (var node in relayNodes) {
                node.StartListening();
            }

            await senderNode.Send();
        }
    }
}
