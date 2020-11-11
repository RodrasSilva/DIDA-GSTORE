using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Client.utils;
using DIDA_GSTORE.ServerService;
using Grpc.Core;
using Grpc.Net.Client;
using Server.grpcService;
using Server.utils;
using ServerDomain;

namespace Server.advancedVersion
{
    public class AdvancedServer
    {
        private float _minDelay;
        private float _maxDelay;
        private string _serverId;
        private string _serverUrl;
        private AdvancedServerStorage _storage;

        public AdvancedServer(float minDelay, float maxDelay, string serverId, string serverUrl)
        {
            _minDelay = minDelay;
            _maxDelay = maxDelay;
            _serverId = serverId;
            _serverUrl = serverUrl;
            _storage = new AdvancedServerStorage();
        }


        public void Run()
        {
            FreezeUtilities freezeUtilities = new FreezeUtilities();
            var serverParameters = UrlParameters.From(_serverUrl);
            var serverService = new ServerService(_storage, freezeUtilities, _serverUrl,DelayMessage);
            var nodeService = new NodeService(freezeUtilities, DelayMessage, RegisterServers, RegisterPartitions);
            var registerSlavesService = new SlaveRegisteringService(_storage);

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var server = new Grpc.Core.Server
            {
                Services = {
                    DIDAService.BindService(serverService),
                    NodeControlService.BindService(nodeService),
                    RegisterSlaveToMasterService.BindService(registerSlavesService),
                    AdvancedSlaveService.BindService( new AdvancedSlaveServerService(_storage))
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

        private void ReadCommands()
        {
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();
        }

        private void RegisterServers(ServerInfo[] serversInfo)
        {
            
            foreach (ServerInfo serverInfo in serversInfo)
            {
                _storage.AddServer(serverInfo.ServerId, serverInfo.ServerUrl);
            }
         
        }

        public void RegisterPartitions(PartitionInfo[] partitionsInfo)
        {
            
            try
            {
                for (var i = 0; i < partitionsInfo.Length; i++)
                {
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
                    if (partitionMasterUrl.Equals(_serverUrl))
                    {
                        Console.WriteLine($"Server [{_serverId}] - Registering as master to partition {partitionId}");

                        _storage.RegisterPartitionMaster(partitionId);
                        continue; // Important, cannot be slave and master to the same partition
                    }

                    try
                    {
                        var request = new RegisterRequest
                        {
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
                    catch (Exception e)
                    {
                        Console.WriteLine(
                            $"Server [{_serverId}] - Error registering as slave to partition {partitionId}. Error message: {e.Message}. {_serverUrl}. {partitionMasterUrl}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine($"Finished server {_serverId} setup");
            Console.WriteLine("----------------------------------------------------");
            
        }

        private void DelayMessage()
        {

            Thread.Sleep(
                Convert.ToInt32((new Random().NextDouble() *
                    (_maxDelay - _minDelay) + _minDelay))
            );
        }
    }

}

