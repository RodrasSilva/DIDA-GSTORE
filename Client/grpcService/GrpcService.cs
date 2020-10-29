using System;
using System.Collections.Generic;
using System.Linq;
using Client;
using Client.model;
using Grpc.Net.Client;

namespace DIDA_GSTORE.grpcService {
    public class GrpcService {
        private const string ObjectNotPresent = "N/A";
        private DIDAService.DIDAServiceClient _client;

        public GrpcService(string serverIp, int serverPort) {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var serverUrl = $"http://{serverIp}:{serverPort}";
            _client = BuildClientFromServerUrl(serverUrl);
        }

        private static DIDAService.DIDAServiceClient BuildClientFromServerUrl(string serverUrl) {
            var channel = GrpcChannel.ForAddress(serverUrl);
            return new DIDAService.DIDAServiceClient(channel);
        }

        private string MapServerIdToUrl(int serverId) {
            ServerUrlRequest request = new ServerUrlRequest {ServerId = serverId};
            return _client.getServerUrl(request).ServerUrl;
        }

        private static ListServerResult MapToListServerResult(ListServerResponseEntity it) {
            return new ListServerResult(it.ObjectValue, it.IsMaster);
        }

        private static ListGlobalResult mapToListGlobalResult(ListGlobalResponseEntity it) {
            return new ListGlobalResult(it.Identifiers.Select(mapToListGlobalResultIdentifier).ToList());
        }

        private static ListGlobalResultIdentifier mapToListGlobalResultIdentifier(ObjectIdentifier it) {
            return new ListGlobalResultIdentifier(it.PartitionId, it.ObjectId);
        }

        public void Write(int partitionId, string objectId, string objectValue) {
            var request = new WriteRequest {PartitionId = partitionId, ObjectId = objectId, ObjectValue = objectValue};
            try {
                var response = _client.write(request);
                switch (response.ResponseCase) {
                    case WriteResponse.ResponseOneofCase.ResponseMessage:
                        break;
                    case WriteResponse.ResponseOneofCase.MasterServerUrl:
                        _client = BuildClientFromServerUrl(response.MasterServerUrl.ServerUrl);
                        Write(partitionId, objectId, objectValue);
                        break;
                    case WriteResponse.ResponseOneofCase.None:
                        break; //  TODO : Check how to handle when none of the above are returned  
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                throw; //  TODO : Check how to handle connection exceptions 
            }
        }


        public string Read(int partitionId, string objectId, int serverId) {
            var request = new ReadRequest {PartitionId = partitionId, ObjectId = objectId};
            try {
                var readResponse = _client.read(request);
                if (!readResponse.ObjectValue.Equals(ObjectNotPresent))
                    return readResponse.ObjectValue;
                if (serverId == -1) return ObjectNotPresent;
                var serverUrl = MapServerIdToUrl(serverId);
                _client = BuildClientFromServerUrl(serverUrl);
                var secondReadResponse = _client.read(request);
                return secondReadResponse.ObjectValue.Equals(ObjectNotPresent)
                    ? ObjectNotPresent
                    : secondReadResponse.ObjectValue;
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                throw; //  TODO : Check how to handle connection exceptions 
            }
        }


        public List<ListServerResult> ListServer(int serverId) {
            var request = new ListServerRequest();
            try {
                var serverUrl = MapServerIdToUrl(serverId);
                _client = BuildClientFromServerUrl(serverUrl);
                var listServerResponse = _client.listServer(request);
                return listServerResponse
                    .Objects
                    .Select(MapToListServerResult)
                    .ToList();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                throw; //  TODO : Check how to handle connection exceptions 
            }
        }

        public List<ListGlobalResult> ListGlobal() {
            ListGlobalRequest request = new ListGlobalRequest();
            try {
                var listServerResponse = _client.listGlobal(request);
                return listServerResponse
                    .Objects
                    .Select(mapToListGlobalResult)
                    .ToList();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                throw; //  TODO : Check how to handle connection exceptions 
            }
        }
    }
}