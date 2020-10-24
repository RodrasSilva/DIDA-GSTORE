using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DIDA_GSTORE.ServerService;


namespace Server
{
    public class Server {

        private static ServerService _serverService = new ServerService();
        private static NodeService _nodeService = new NodeService();
        const int Port = 5001;

        static void Main(string[] args) {
            /*Grpc.Core.Server server = new Grpc.Core.Server
            {
                Services = { DIDAService.BindService(_serverService),
                    //NodeControlService.BindService(_nodeService), 
                },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };*/
            Grpc.Core.Server server = new Grpc.Core.Server
            {
                Services = { //DIDAService.BindService(_serverService),
                    NodeControlService.BindService(_nodeService), 
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
