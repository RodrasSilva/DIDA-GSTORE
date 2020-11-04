using System;
using Grpc.Net.Client;
using PuppetMasterClient;

//using Client;

namespace DIDA_GSTORE.grpcService {
    public class GrpcNodeService {
        private readonly GrpcChannel channel;
        private readonly NodeControlService.NodeControlServiceClient client;
        private string ServerIp;
        private int ServerPort;
        public string Url { get; set; }
        //inves de ip, url
        public GrpcNodeService(string serverIp, int serverPort) {
            ServerIp = serverIp;
            ServerPort = serverPort;
            Url = ServerIp + ":" + serverPort;
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            channel = GrpcChannel.ForAddress(BuildServerAdress(serverIp, serverPort));
            client = new NodeControlService.NodeControlServiceClient(channel);
        }

        private string BuildServerAdress(string serverIp, int serverPort) {
            return string.Format("http://{0}:{1}", serverIp, serverPort);
        }

        public StatusResponse Status() {
            var request = new StatusRequest();
            return client.status(request);
        }

        public CrashResponse Crash() {
            var request = new CrashRequest();
            return client.crash(request);
        }

        public FreezeResponse Freeze() {
            var request = new FreezeRequest();
            return client.freeze(request);
        }

        public UnfreezeResponse Unfreeze()
        {
            var request = new UnfreezeRequest();
            return client.unfreeze(request);
        }

        public CompleteSetupResponse CompleteSetup()
        {
            var request = new CompleteSetupRequest();
            return client.completeSetup(request);
        }
    }
}