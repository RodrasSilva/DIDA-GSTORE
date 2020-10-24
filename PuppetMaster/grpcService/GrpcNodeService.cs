using System;
//using Client;
using PuppetMasterClient;
using Grpc.Net.Client;

namespace DIDA_GSTORE.grpcService {
    public class GrpcNodeService
    {
        private string ServerIp;
        private int ServerPort;
        private GrpcChannel channel;
        private NodeControlService.NodeControlServiceClient client;

        //inves de ip, url
        public GrpcNodeService(string serverIp, int serverPort)
        {
            ServerIp = serverIp;
            ServerPort = serverPort;
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            channel = GrpcChannel.ForAddress(BuildServerAdress(serverIp, serverPort));
            client = new NodeControlService.NodeControlServiceClient(channel);
        }

        private string BuildServerAdress(string serverIp, int serverPort)
        {
            return string.Format("http://{0}:{1}", serverIp, serverPort);
        }
        public StatusResponse Status()
        {
            StatusRequest request = new StatusRequest() {};
            return client.status(request);
        }
        public CrashResponse Crash()
        {
            CrashRequest request = new CrashRequest() {};
            return client.crash(request);
        }

        public FreezeResponse Freeze()
        {
            FreezeRequest request = new FreezeRequest() { };
            return client.freeze(request);
        }
        public UnfreezeResponse Unfreeze()
        {
            UnfreezeRequest request = new UnfreezeRequest() { };
            return client.unfreeze(request);
        }
    }
}