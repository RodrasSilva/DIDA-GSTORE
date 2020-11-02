using System;
using Grpc.Net.Client;
//using Client;
using PuppetMasterClient;

namespace DIDA_GSTORE.grpcService
{
    public class GrpcProcessService
    {
        private string ServerIp;
        private int ServerPort;
        private GrpcChannel channel;
        private ProcessCreationService.ProcessCreationServiceClient client;

        public GrpcProcessService(string serverIp, int serverPort)
        {
            ServerIp = serverIp;
            ServerPort = serverPort;
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            channel = GrpcChannel.ForAddress(BuildServerAdress(serverIp, serverPort));
            client = new ProcessCreationService.ProcessCreationServiceClient(channel);
        }

        private string BuildServerAdress(string serverIp, int serverPort)
        {
            return string.Format("http://{0}:{1}", serverIp, serverPort);
        }

        public StartServerResponse StartServer(int serverId, string url,
            int minDelay, int maxDelay)
        {
            StartServerRequest request = new StartServerRequest()
                {ServerId = serverId, URL = url, MinDelay = minDelay, MaxDelay = maxDelay};
            return client.startServer(request); // TODO :  Add Logic
            //start server async <= probably not
        }

        public StartClientResponse StartClient(string username, string url, string scriptFile)
        {
            StartClientRequest request = new StartClientRequest()
                {Username = username, URL = url, ScriptFile = scriptFile};
            return client.startClient(request); // TODO :  Add Logic
        }
    }
}