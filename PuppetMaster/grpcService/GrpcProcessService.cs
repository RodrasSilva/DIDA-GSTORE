using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Grpc.Net.Client;
using PuppetMasterClient;
using PuppetMasterMain;

//using Client;

namespace DIDA_GSTORE.grpcService {
    public class GrpcProcessService {
        private readonly GrpcChannel channel;
        private readonly ProcessCreationService.ProcessCreationServiceClient client;

        public GrpcProcessService(string serverIp, int serverPort) {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            channel = GrpcChannel.ForAddress(BuildServerAdress(serverIp, serverPort));
            client = new ProcessCreationService.ProcessCreationServiceClient(channel);
        }

        private string BuildServerAdress(string serverIp, int serverPort) {
            return string.Format("http://{0}:{1}", serverIp, serverPort);
        }

        public StartServerResponse StartServer(string serverId, string url,
            float minDelay, float maxDelay) {
            var request = new StartServerRequest {
                ServerId = serverId,
                URL = url,
                MinDelay = minDelay,
                MaxDelay = maxDelay
            };
            return client.startServer(request);
        }

        public StartClientResponse StartClient(string username,
            string url, string scriptFile, string defaultServerUrl, List<PartitionClientMessage> partitions) {
            var request = new StartClientRequest {
                Username = username, URL = url, ScriptFile = scriptFile,
                DefaultServerUrl = defaultServerUrl,
                Partitions = {partitions}
            };
            return client.startClient(request);
        }
    }
}