using System;
using DIDA_GSTORE.ServerService;
using Grpc.Core;

namespace Client.clientNodeServer {
    public class ClientNodeServer {
        private readonly Server _server;

        public ClientNodeServer(string host, int port, ServerCredentials credentials) {
            _server = new Server {
                Services = {
                    NodeControlService.BindService(new NodeService()),
                },
                Ports = {new ServerPort(host, port, credentials)}
            };
        }

        public void Start() {
            _server.Start();
        }

        public void ShutdownAsync() {
            _server.ShutdownAsync().Wait();
        }
    }
}