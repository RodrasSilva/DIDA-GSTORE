using System;
using DIDA_GSTORE.ServerService;
using Grpc.Core;

namespace Client.clientNodeServer {
    public class ClientNodeServer {
        private readonly string _host;
        private readonly int _port;
        private readonly ServerCredentials _credentials;

        public ClientNodeServer(string host, int port, ServerCredentials credentials) {
            _host = host;
            _port = port;
            _credentials = credentials;
        }

        public void Start(NodeService nodeService) {
            Server server = new Server {
                Services = {
                    NodeControlService.BindService(nodeService),
                },
                Ports = {new ServerPort(_host, _port, _credentials)}
            };
            server.Start();
            Console.WriteLine("ChatClient listening to status commands on port " + _port);
            server.ShutdownAsync().Wait();
        }
    }
}