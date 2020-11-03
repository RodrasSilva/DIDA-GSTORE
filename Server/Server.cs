using System;
using System.Linq;
using System.Threading;
using Client.utils;
using DIDA_GSTORE.ServerService;
using Grpc.Core;
using Server.grpcService;

namespace ServerDomain {
    public class Server {
        private const bool UseBaseVersion = true;
        private static float _minDelay;
        private static float _maxDelay;

        public static void Main(string[] args) {
            if (args.Length < 5 || args.Length % 2 != 0) {
                Console.WriteLine(
                    "Usage: Server <id> <url> <minDelay> <maxDelay> <partitionId1> <partitionMaster1Url> ... <partitionIdN> <partitionMasterNUrl>");
                return;
            }

            var serverId = args[0];
            var url = args[1];
            _minDelay = float.Parse(args[2]);
            _maxDelay = float.Parse(args[3]);

            var partitionArgs = args.Take(4).ToArray();

            var counter = 0;

            IStorage storage;
            if (UseBaseVersion) {
                storage = new BaseServerStorage();
            }
            else {
#pragma warning disable CS0162 // Unreachable code detected
                storage = new AdvancedServerStorage();
#pragma warning restore CS0162 // Unreachable code detected
            }

            FillPartitionsFromArgs(partitionArgs, storage);
            var serverParameters = UrlParameters.From(url);
            var serverService = new ServerService(storage);
            var nodeService = new NodeService();
            var registerSlavesService = new SlaveRegisteringService(storage);


            var server = new Grpc.Core.Server {
                Services = {
                    DIDAService.BindService(serverService),
                    NodeControlService.BindService(nodeService),
                    RegisterSlaveToMasterService.BindService(registerSlavesService),
                    UseBaseVersion
                        ? BaseSlaveService.BindService(new BaseSlaveServerService(serverId, url, partitionArgs,
                            (BaseServerStorage) storage))
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

        private static void ReadCommands() {
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();
        }

        private static void FillPartitionsFromArgs(string[] args, IStorage storage) {
            //ignore first 3 indexes
            for (var index = 3; index < args.Length; index++) {
                var partitionId = int.Parse(args[index]);
                index++;
                var master = bool.Parse(args[index]);

                //storage.Partitions.Add(partitionId, master);
                //something along these lines
            }
        }


        public static void DelayMessage() {
            Thread.Sleep(
                Convert.ToInt32((new Random().NextDouble() *
                    (_maxDelay - _minDelay) + _minDelay) * 1000)
            );
        }
    }
}