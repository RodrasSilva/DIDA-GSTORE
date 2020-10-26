using System;
using DIDA_GSTORE.ServerService;
using Grpc.Core;

namespace Client.clientNodeServer {
    public class ClientNodeServer {
        private readonly string _host;
        private readonly int _port;
        private readonly ServerCredentials _credentials;
        private readonly NodeService _nodeService;

        public ClientNodeServer(string host, int port, ServerCredentials credentials,NodeService nodeService) {
            _host = host;
            _port = port;
            _credentials = credentials;
            _nodeService = nodeService;
        }

        public void Start() {
            Server server = new Server {
                Services = {
                    NodeControlService.BindService(_nodeService),
                },
                Ports = {new ServerPort(_host, _port, _credentials)}
            };
            server.Start();
            Console.WriteLine("ChatClient listening to status commands on port " + _port);
            server.ShutdownAsync().Wait();
        }
    }
}