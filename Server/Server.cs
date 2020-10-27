using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DIDA_GSTORE.ServerService;
using DIDA_GSTORE.SlaveServerService;
using Server.storage;

namespace Server
{
    public class Server {

        public static void Main(string[] args) {

            Storage storage = new Storage();
        ServerService _serverService = new ServerService(storage);
        SlaveServerService slaveService = new SlaveServerService(storage);
        NodeService _nodeService = new NodeService();
        int Port = 5001;
            Grpc.Core.Server server = new Grpc.Core.Server
            {
                Services = { DIDAService.BindService(_serverService),
                    NodeControlService.BindService(_nodeService),
                    SlaveService.BindService(slaveService)

                },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();
            Console.WriteLine("ChatServer server listening on port " + Port);
            ReadCommands();

            server.ShutdownAsync().Wait();
        }

        private static void ReadCommands() {
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();
        }

    }
}
