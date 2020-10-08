using System;
using Client;
using Grpc.Net.Client;

namespace DIDA_GSTORE.grpcService
{
    public class GrpcService
    {
        private string ServerIp;
        private int ServerPort;
        private GrpcChannel channel;
        private DIDAService.DIDAServiceClient client;

        public GrpcService(string serverIp, int serverPort)
        {
            ServerIp = serverIp;
            ServerPort = serverPort;
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            channel = GrpcChannel.ForAddress(buildServerAdress(serverIp, serverPort));
            client = new DIDAService.DIDAServiceClient(channel);
        }

        private string buildServerAdress(string serverIp, int serverPort)
        {
            return string.Format("http://{0}:{1}", serverIp, serverPort);
        }

        public WriteResponse Write(string partitionId, string objectId, string objectValue)
        {
            WriteRequest request = new WriteRequest() { PartitionId = partitionId, ObjectId = objectId, ObjectValue = objectValue };
            return client.write(request); // TODO :  Add Logic
        }

        public ReadResponse Read(string partitionId, string objectId, string serverId)
        {
            ReadRequest request = new ReadRequest() { PartitionId = partitionId, ObjectId = objectId };
            return client.read(request); // TODO :  Add Logic

        }

        public ListServerResponse ListServer(string serverId)
        {
            ListServerRequest request = new ListServerRequest();
            return client.listServer(request); // TODO :  Add Logic
        }

        public ListGlobalResponse ListGlobal()
        {
            ListGlobalRequest request = new ListGlobalRequest();
            return client.listGlobal(request); // TODO :  Add Logic
        }


    }
}