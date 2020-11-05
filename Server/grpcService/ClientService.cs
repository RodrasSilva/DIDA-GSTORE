using System;
using System.Collections.Generic;
using System.Linq;
using Grpc.Net.Client;

namespace ServerDomain
{
    public class ClientService {

        private DIDAService.DIDAServiceClient _client;

        public ClientService(string serverUrl){
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            _client = BuildClientFromServerUrl(serverUrl);
        }

        private static DIDAService.DIDAServiceClient BuildClientFromServerUrl(string serverUrl){
            var channel = GrpcChannel.ForAddress(serverUrl);
            return new DIDAService.DIDAServiceClient(channel);
        }

        public ListPartitionGlobalResponse ListPartitionGlobal(string partitionId)
        {
            return _client.listPartitionGlobal(new ListPartitionGlobalRequest {
                PartitionId = partitionId 
            });
        }
    }
}