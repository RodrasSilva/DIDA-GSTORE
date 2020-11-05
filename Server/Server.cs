using System;
using System.Linq;
using System.Threading;
using Client.utils;
using DIDA_GSTORE.ServerService;
using Grpc.Core;
using Grpc.Net.Client;
using Server.grpcService;

namespace ServerDomain{
    public class Server{
        private const bool UseBaseVersion = true;
        private static float _minDelay;
        private static float _maxDelay;
        private static string _serverId;
        private static string _serverUrl;
        private static IStorage _storage;


        private static void ParseArgs(string[] args){
            if (args.Length != 4){
                Console.WriteLine("You gave: " + string.Join(" ", args) +
                                  ". Usage is: Server <id> <url> <minDelay> <maxDelay>");
                return;
            }

            _serverId = args[0];
            _serverUrl = args[1];
            _minDelay = float.Parse(args[2]);
            _maxDelay = float.Parse(args[3]);

           
        }

        public static void Main(string[] args){
            ParseArgs(args);

            if (UseBaseVersion){
                _storage = new BaseServerStorage();
            }
            else{
#pragma warning disable CS0162 // Unreachable code detected
                _storage = new AdvancedServerStorage();
#pragma warning restore CS0162 // Unreachable code detected
            }

            Console.WriteLine(_serverUrl);
            var serverParameters = UrlParameters.From(_serverUrl);
            var serverService = new ServerService(_storage, _serverUrl);
            var nodeService = new NodeService();
            var registerSlavesService = new SlaveRegisteringService(_storage);

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
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
            Console.WriteLine("Server " + _serverId + " listening on port " + serverParameters.Port);
            ReadCommands();

            server.ShutdownAsync().Wait();
        }

        private static void ReadCommands(){
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();
        }

        public static void RegisterServers(ServerInfo[] serversInfo)
        {
            foreach(ServerInfo serverInfo in serversInfo)
            {
                _storage.AddServer(serverInfo.ServerId, serverInfo.ServerUrl);
            }
        }

        public static void RegisterPartitions(PartitionInfo[] partitionsInfo){
            //this string is assumed to always be 
            //[partitionId.1, partitionMasterUrl.1, ...., partitionId.N, partitionMasterUrl.N]

            try{
                for (var i = 0; i < partitionsInfo.Length; i++){
                    var partitionId = partitionsInfo[i].PartitionId;
                    var partitionMasterUrl = partitionsInfo[i].PartitionMasterUrl;
                    var isMyPartition = partitionsInfo[i].IsMyPartition;

                    
                    _storage.AddPartition(partitionId, partitionMasterUrl);
                    if (!isMyPartition)
                    {
                        Console.WriteLine($"Server [{_serverId}]  - Partition {partitionId} placeholder registered");
                        continue;
                    }
                    Console.WriteLine($"Server [{_serverId}] - Adding to partition " + partitionId +
                                                          " with master url = " + partitionMasterUrl);
                    if (partitionMasterUrl.Equals(_serverUrl)){
                        Console.WriteLine($"Server [{_serverId}] - Registering as master to partition {partitionId}");

                        _storage.RegisterPartitionMaster(partitionId);
                        continue; // Important, cannot be slave and master to the same partition
                    }

                    try{
                        var request = new RegisterRequest {
                            ServerId = _serverId,
                            Url = _serverUrl,
                            PartitionId = partitionId
                        };

                        Console.WriteLine($"Server [{_serverId}] - Registering as slave to partition {partitionId}");
                        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                        var channel = GrpcChannel.ForAddress(partitionMasterUrl);
                        var client = new RegisterSlaveToMasterService.RegisterSlaveToMasterServiceClient(channel);

                        client.registerAsSlave(request);
                    }
                    catch (Exception e){
                        Console.WriteLine(
                            $"Server [{_serverId}] - Error registering as slave to partition {partitionId}. Error message: {e.Message}. {_serverUrl}. {partitionMasterUrl}");
                    }
                }
            }
            catch (Exception e){
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine($"Finished server {_serverId} setup");
            Console.WriteLine("----------------------------------------------------");
        }

        public static void DelayMessage(){
            Thread.Sleep(
                Convert.ToInt32((new Random().NextDouble() *
                    (_maxDelay - _minDelay) + _minDelay))
            );
        }
    }
}