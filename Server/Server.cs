using Grpc.Core;
using System;
using DIDA_GSTORE.ServerService;
using DIDA_GSTORE.SlaveServerService;
using Server.storage;
using System.Threading;

namespace ServerDomain
{
    public class Server {

        private static float _minDelay;
        private static float _maxDelay;
        public static void Main(string[] args) {

            if (args.Length < 4 || args.Length % 2 != 1)
            {
                Console.WriteLine("Usage: Server <url> <minDelay> <maxDelay> <partitionId1> <partitionMaster1> ... <partitionIdN> <partitionMasterN>");
                return;
            }
            string url = args[0];
            float minDelay = float.Parse(args[1]);
            float maxDelay = float.Parse(args[2]);
            _minDelay = minDelay;
            _maxDelay = maxDelay;

            Storage storage = new Storage();
            FillPartitionsFromArgs(args, storage);

            ServerService _serverService = new ServerService(storage);
            SlaveServerServiceServer _slaveService = new SlaveServerServiceServer(storage);

            NodeService _nodeService = new NodeService();
        
            int Port = 5001;
            Grpc.Core.Server server = new Grpc.Core.Server
            {
                Services = { DIDAService.BindService(_serverService),
                    NodeControlService.BindService(_nodeService),
                    SlaveService.BindService(_slaveService)

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

        private static void FillPartitionsFromArgs(string[] args, Storage storage)
        {
            //ignore first 3 indexes
            for(int index = 3; index < args.Length; index++)
            {
                int partitionId = int.Parse(args[index]);
                index++;
                bool master = bool.Parse(args[index]);
                
                //storage.Partitions.Add(partitionId, master);
                //something along these lines
            }
        }

        public static void DelayMessage()
        {
            Thread.Sleep(
                Convert.ToInt32 ( (new Random().NextDouble() * 
                (_maxDelay - _minDelay) + _minDelay) * 1000)
            );
        }
    }
}
