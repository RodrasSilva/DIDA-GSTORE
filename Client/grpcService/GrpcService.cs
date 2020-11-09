using System;
using System.Collections.Generic;
using System.Linq;
using Client;
using Client.model;
using Grpc.Net.Client;

namespace DIDA_GSTORE.grpcService{
    public class GrpcService{
        private const string ObjectNotPresent = "N/A";
        private DIDAService.DIDAServiceClient _client;

        public GrpcService(string serverIp, int serverPort){
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var serverUrl = $"http://{serverIp}:{serverPort}";
            _client = BuildClientFromServerUrl(serverUrl);
        }

        private static DIDAService.DIDAServiceClient BuildClientFromServerUrl(string serverUrl){
            var channel = GrpcChannel.ForAddress(serverUrl);
            return new DIDAService.DIDAServiceClient(channel);
        }

        private string MapServerIdToUrl(string serverId){
            var request = new ServerUrlRequest{ServerId = serverId};
            return _client.getServerUrl(request).ServerUrl;
        }

        private static ListServerResult MapToListServerResult(ListServerResponseEntity it){
            return new ListServerResult(it.ObjectId, it.ObjectValue, it.IsMaster);
        }
        /*
        private static ListGlobalResult mapToListGlobalResult(ListGlobalResponse it){
            return new ListGlobalResult(it.Objects.Select(mapToListGlobalResultIdentifier).ToList());
        }

        private static ListGlobalResultIdentifier mapToListGlobalResultIdentifier(ObjectIdentifier it){
            return new ListGlobalResultIdentifier(it.PartitionId, it.ObjectId);
        }
        */
        public void Write(string partitionId, string objectId, string objectValue){
            var request = new WriteRequest{PartitionId = partitionId, ObjectId = objectId, ObjectValue = objectValue};
            try{
                var response = _client.write(request);
                //Console.WriteLine("Am i here?");
                switch (response.ResponseCase){
                    case WriteResponse.ResponseOneofCase.ResponseMessage:
                        Console.WriteLine("Write Successful");
                        break;
                    case WriteResponse.ResponseOneofCase.MasterServerUrl:
                        Console.WriteLine($"Write - Changing to server {response.MasterServerUrl.ServerUrl}");
                        _client = BuildClientFromServerUrl(response.MasterServerUrl.ServerUrl);
                        Write(partitionId, objectId, objectValue);
                        break;
                    case WriteResponse.ResponseOneofCase.None:
                        Console.WriteLine("TO DO CASE");
                        break; //  TODO : Check how to handle when none of the above are returned  
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e){

                Console.WriteLine("Error Writing");

                Console.WriteLine(e.Message);
                Console.ReadLine();
                //throw; //  TODO : Check how to handle connection exceptions 
            }
        }


        public string Read(string partitionId, string objectId, string serverId){
            var request = new ReadRequest{PartitionId = partitionId, ObjectId = objectId};
            try{
                var readResponse = _client.read(request);
                if (!readResponse.ObjectValue.Equals(ObjectNotPresent))
                    return readResponse.ObjectValue;
                if (serverId.Equals("-1")) return ObjectNotPresent;
                var serverUrl = MapServerIdToUrl(serverId);
                _client = BuildClientFromServerUrl(serverUrl);
                var secondReadResponse = _client.read(request);
                return secondReadResponse.ObjectValue.Equals(ObjectNotPresent)
                    ? ObjectNotPresent
                    : secondReadResponse.ObjectValue;
            }
            catch (Exception e){
                Console.WriteLine("Error reading");
                Console.WriteLine(e.Message);
                
                Console.ReadLine();
                return "Error";
                //throw; //  TODO : Check how to handle connection exceptions 
            }
        }


        public List<ListServerResult> ListServer(string serverId){
            var request = new ListServerRequest();
            try{
                var serverUrl = MapServerIdToUrl(serverId);
                _client = BuildClientFromServerUrl(serverUrl);
                var listServerResponse = _client.listServer(request);
                return listServerResponse
                    .Objects
                    .Select(MapToListServerResult)
                    .ToList();
            }
            catch (Exception e){
                Console.WriteLine("Error List Server");
                Console.WriteLine(e.Message);
                Console.ReadLine();
                //Console.WriteLine(e.ToString());
                throw; //  TODO : Check how to handle connection exceptions 
            }
        }

        public List<ListGlobalResponseEntity> ListGlobal(){
            var request = new ListGlobalRequest();
            try{
                var listServerResponse = _client.listGlobal(request);
                //Console.WriteLine(listServerResponse.Objects.Count);
                return listServerResponse.Objects.ToList();
            }
            catch (Exception e){
                Console.WriteLine("List Global Error");
                Console.WriteLine(e.Message);
              
                Console.ReadLine();
                //Console.WriteLine(e.ToString());
                throw; //  TODO : Check how to handle connection exceptions 
            }
        }
    }
}