using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Net.Client;
using PuppetMasterClient;
using PuppetMasterMain;

//using Client;

namespace DIDA_GSTORE.grpcService {
    public class GrpcNodeService {
        private readonly GrpcChannel channel;
        private readonly NodeControlService.NodeControlServiceClient client;
        private readonly string ServerIp;

        private int ServerPort;

        //inves de ip, url
        public GrpcNodeService(string serverIp, int serverPort) {
            ServerIp = serverIp;
            ServerPort = serverPort;
            Url = ServerIp + ":" + serverPort;
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            channel = GrpcChannel.ForAddress(BuildServerAdress(serverIp, serverPort));
            client = new NodeControlService.NodeControlServiceClient(channel);
        }

        public string Url { get; set; }

        private string BuildServerAdress(string serverIp, int serverPort) {
            return string.Format("http://{0}:{1}", serverIp, serverPort);
        }

        public StatusResponse Status() {
            var request = new StatusRequest();
            try {
                return client.status(request);
            }
            catch (RpcException e) {
                Console.WriteLine("Status Did not work");
                Console.WriteLine(e.Message);
                return new StatusResponse();
            }
        }

        public CrashResponse Crash() {
            var request = new CrashRequest();
            return client.crash(request);
        }

        public FreezeResponse Freeze(bool discard) {
            var request = new FreezeRequest {Discard = discard};
            return client.freeze(request);
        }

        public UnfreezeResponse Unfreeze() {
            var request = new UnfreezeRequest();
            return client.unfreeze(request);
        }

        public CompleteSetupResponse CompleteSetup(
            List<ServerInfoMessage> serverInfoMessages,
            List<PartitionInfoMessage> partitionInfoMessages) {
            var request = new CompleteSetupRequest {
                ServerInfo = {serverInfoMessages},
                Partitions = {partitionInfoMessages}
            };

            Console.Write(request.ToString());

            return client.completeSetup(request);
        }
    }
}