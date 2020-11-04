using System;
using System.Linq;
using System.Threading;
using Client.utils;
using DIDA_GSTORE.ServerService;
using Grpc.Core;
using Grpc.Net.Client;
using Server.grpcService;

namespace ServerDomain {
    public class Server {
        private const bool UseBaseVersion = true;
        private static float _minDelay;
        private static float _maxDelay;
        private static string _serverId;
        private static string _serverUrl;
        private static string[] _partitions;
        private static IStorage _storage;


        private static void ParseArgs(string[] args)
        {
            if (args.Length < 5 || args.Length % 2 != 0)
            {
                Console.WriteLine("You gave: " + string.Join(" ",args) +
                    ". Usage is: Server <id> <url> <minDelay> <maxDelay> <partitionId1> <partitionMaster1Url> ... <partitionIdN> <partitionMasterNUrl>");
                return;
            }

            _serverId = args[0];
            _serverUrl = args[1];
            _minDelay = float.Parse(args[2]);
            _maxDelay = float.Parse(args[3]);

            _partitions = args.Take(4).ToArray();
        }
        public static void Main(string[] args) {
            ParseArgs(args);

            if (UseBaseVersion) {
                _storage = new BaseServerStorage();
            }
            else {
#pragma warning disable CS0162 // Unreachable code detected
                _storage = new AdvancedServerStorage();
#pragma warning restore CS0162 // Unreachable code detected
            }

            Console.WriteLine(_serverUrl);
            var serverParameters = UrlParameters.From(_serverUrl);
            var serverService = new ServerService(_storage);
            var nodeService = new NodeService();
            var registerSlavesService = new SlaveRegisteringService(_storage);

            FillPartitionsFromArgs();

            var server = new Grpc.Core.Server {
                Services = {
                    DIDAService.BindService(serverService),
                    NodeControlService.BindService(nodeService),
                    RegisterSlaveToMasterService.BindService(registerSlavesService),
                    UseBaseVersion
                        ? BaseSlaveService.BindService(
                            new BaseSlaveServerService((BaseServerStorage) _storage)
                        )
                        : AdvancedSlaveService.BindService(
                            new AdvancedSlaveServerService((AdvancedServerStorage) _storage)
                        )
                },
                Ports = {
                    new ServerPort(serverParameters.Hostname,
                    serverParameters.Port, ServerCredentials.Insecure)
                }
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
        private static void FillPartitionsFromArgs()
        {
            for (var index = 0; index < _partitions.Length; index+=2)
            {
                var partitionId = _partitions[index];
                var masterUrl = _partitions[index + 1];

                _storage.AddPartition(partitionId, masterUrl);
            }
        }

        public static void RegisterPartitions()
        {
            //this string is assumed to always be 
            //[partitionId.1, partitionMasterUrl.1, ...., partitionId.N, partitionMasterUrl.N]
            for (var i = 0; i < _partitions.Length; i += 2)
            {
                var partitionId = _partitions[i];
                var partitionMasterUrl = _partitions[i + 1];

                if (partitionMasterUrl.Equals(_serverUrl))
                {
                    _storage.RegisterPartitionMaster(partitionId);
                    continue; // Important, cannot be slave and master to the same partition
                }
                try
                {
                    var request = new RegisterRequest { 
                        ServerId = _serverId,
                        Url = _serverUrl,
                        PartitionId = partitionId 
                    };

                    Console.WriteLine($"Registering as slave to partition {partitionId}");
                    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                    var channel = GrpcChannel.ForAddress(partitionMasterUrl);
                    var client = new RegisterSlaveToMasterService.
                        RegisterSlaveToMasterServiceClient(channel);

                    client.registerAsSlave(request);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Error registering as slave to partition {partitionId}");
                }
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