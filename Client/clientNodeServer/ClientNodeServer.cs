using System;
using DIDA_GSTORE.ServerService;
using Grpc.Core;

namespace Client.clientNodeServer {
    public class ClientNodeServer {
        private Server server;
        private int _port;

        public ClientNodeServer(string host, int port, ServerCredentials credentials, NodeService nodeService) {
            _port = port;
            server = new Server
            {
                Services = {
                    NodeControlService.BindService(nodeService),
                },
                Ports = { new ServerPort(host, port, credentials) }
            };
        }

        public void Start() {
            server.Start();
            Console.WriteLine("ChatClient listening to status commands on port " + _port);
            server.ShutdownAsync().Wait();
        }

        public void ShutdownAsync() {
            server.ShutdownAsync().Wait();
        }
    }
}