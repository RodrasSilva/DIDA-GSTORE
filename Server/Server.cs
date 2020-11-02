using System;
using System.Threading;
using Client.utils;
using DIDA_GSTORE.ServerService;
using Grpc.Core;

namespace ServerDomain
{
    public class Server
    {
        private const bool UseBaseVersion = true;
        private static float _minDelay;
        private static float _maxDelay;

        public static void Main(string[] args)
        {
            if (args.Length < 4 || args.Length % 2 != 1)
            {
                Console.WriteLine(
                    "Usage: Server <url> <minDelay> <maxDelay> <partitionId1> <partitionMaster1Url> ... <partitionIdN> <partitionMasterNUrl>");
                return;
            }

            string url = args[0];
            float minDelay = float.Parse(args[1]);
            float maxDelay = float.Parse(args[2]);

            IStorage storage;
            if (UseBaseVersion)
            {
                storage = new BaseServerStorage();
            }
            else
            {
#pragma warning disable CS0162 // Unreachable code detected
                storage = new AdvancedServerStorage();
#pragma warning restore CS0162 // Unreachable code detected
            }

            FillPartitionsFromArgs(args, storage);
            UrlParameters serverParameters = UrlParameters.From(url);
            ServerService _serverService = new ServerService(storage);
            NodeService _nodeService = new NodeService();


            Grpc.Core.Server server = new Grpc.Core.Server
            {
                Services =
                {
                    DIDAService.BindService(_serverService),
                    NodeControlService.BindService(_nodeService),
                    UseBaseVersion
                        ? BaseSlaveService.BindService(new BaseSlaveServerService((BaseServerStorage) storage))
                        : AdvancedSlaveService.BindService(
                            new AdvancedSlaveServerService((AdvancedServerStorage) storage))
                },
                Ports = {new ServerPort(serverParameters.Hostname, serverParameters.Port, ServerCredentials.Insecure)}
            };
            server.Start();
            Console.WriteLine("ChatServer server listening on port " + serverParameters.Port);
            ReadCommands();

            server.ShutdownAsync().Wait();
        }

        private static void ReadCommands()
        {
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();
        }

        private static void FillPartitionsFromArgs(string[] args, IStorage storage)
        {
            //ignore first 3 indexes
            for (int index = 3; index < args.Length; index++)
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
                Convert.ToInt32((new Random().NextDouble() *
                    (_maxDelay - _minDelay) + _minDelay) * 1000)
            );
        }
    }
}