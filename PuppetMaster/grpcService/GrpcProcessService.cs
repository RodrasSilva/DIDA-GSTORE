using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Grpc.Net.Client;
using PuppetMasterClient;
using PuppetMasterMain;

//using Client;

namespace DIDA_GSTORE.grpcService{
    public class GrpcProcessService{
        private readonly GrpcChannel channel;
        private readonly ProcessCreationService.ProcessCreationServiceClient client;
        private string ServerIp;
        private int ServerPort;

        public GrpcProcessService(string serverIp, int serverPort){
            ServerIp = serverIp;
            ServerPort = serverPort;
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            channel = GrpcChannel.ForAddress(BuildServerAdress(serverIp, serverPort));
            client = new ProcessCreationService.ProcessCreationServiceClient(channel);
        }

        private string BuildServerAdress(string serverIp, int serverPort){
            return string.Format("http://{0}:{1}", serverIp, serverPort);
        }

        public StartServerResponse StartServer(string serverId, string url,
            float minDelay, float maxDelay, List<PartitionInfo> partitionInfos){
            var partitionMessages =
                new RepeatedField<PartitionMessage>();
            foreach (var partitionInfo in partitionInfos){
                var partitionMessage = new PartitionMessage {
                    Id = partitionInfo.partitionId,
                    MasterURL = partitionInfo.masterUrl
                };
                partitionMessages.Add(partitionMessage);
            }

            var request = new StartServerRequest {
                ServerId = serverId,
                URL = url,
                MinDelay = minDelay,
                MaxDelay = maxDelay,
                Partitions = {partitionMessages}
            };
            return client.startServer(request); // TODO :  Add Logic
            //start server async <= probably not
        }

        public StartClientResponse StartClient(string username,
            string url, string scriptFile, string defaultServerUrl){
            var request = new StartClientRequest {
                Username = username, URL = url, ScriptFile = scriptFile,
                DefaultServerUrl = defaultServerUrl
            };
            return client.startClient(request); // TODO :  Add Logic
        }
    }
}