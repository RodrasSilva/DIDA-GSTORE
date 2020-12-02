using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Server.utils;
using ServerDomain;

namespace DIDA_GSTORE.ServerService {
    public class ServerService : DIDAService.DIDAServiceBase {
        private readonly IStorage _storage;
        private readonly FreezeUtilities _freezeUtilities;
        private string _myUrl;
        private Action delayFunction;

        public ServerService(IStorage storage, FreezeUtilities freezeUtilities, string myUrl, Action delayFunction) {
            _storage = storage;
            _freezeUtilities = freezeUtilities;
            _myUrl = myUrl;
            this.delayFunction = delayFunction;
        }


        public override Task<WriteResponse> write(WriteRequest request, ServerCallContext context) {
            _freezeUtilities.WaitForUnfreeze();
            delayFunction();
            return Task.FromResult(Write(request));
        }

        private WriteResponse Write(WriteRequest request) {
            Console.WriteLine("Received Write request. Id - " +
                              request.ObjectId + ". Value" + request.ObjectValue);
            try {
                var partitionId = request.PartitionId;
                var objectId = request.ObjectId;
                var objectValue = request.ObjectValue;

                var partition = _storage.GetPartitionOrThrowException(partitionId);
                if (partition.IsMaster) {
                    Console.WriteLine("I'm the master of this partition");
                    _storage.WriteMaster(partitionId, objectId, objectValue, -1);
                    return new WriteResponse {ResponseMessage = "OK"};
                }

                Console.WriteLine("I'm not the master of this partition");

                var url = _storage.GetMasterUrl(request.PartitionId);
                return new WriteResponse {MasterServerUrl = new ServerUrlResponse {ServerUrl = url}};
            }
            catch (Exception e) {
                Console.WriteLine("I don't have the partition");

                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                Console.WriteLine("End of exception");
                return new WriteResponse();
            }
        }

        public override Task<ReadResponse> read(ReadRequest request, ServerCallContext context) {
            _freezeUtilities.WaitForUnfreeze();
            delayFunction();
            return Task.FromResult(Read(request));
        }

        public override Task<ReadAdvancedResponse>
            readAdvanced(ReadAdvancedRequest request, ServerCallContext context) {
            _freezeUtilities.WaitForUnfreeze();
            delayFunction();
            return Task.FromResult(ReadAdvanced(request));
        }


        private ReadResponse Read(ReadRequest request) {
            Console.WriteLine("Received Read Request: " + request.ToString());
            try {
                var partitionId = request.PartitionId;
                var objectId = request.ObjectId;
                var partition = _storage.GetPartitionOrThrowException(partitionId);
                var objectValue = _storage.Read(partitionId, objectId);
                var response = new ReadResponse {ObjectValue = objectValue};
                return response;
            }
            catch (Exception) {
                /*Partition not founded */
                return new ReadResponse {ObjectValue = "N/A"};
            }
        }

        private ReadAdvancedResponse ReadAdvanced(ReadAdvancedRequest request) {
            Console.WriteLine("Received Read Request: " + request.ToString());
            try {
                var partitionId = request.PartitionId;
                var objectId = request.ObjectId;

                var partition = _storage.GetPartitionOrThrowException(partitionId);
                var objectValue = _storage.ReadAdvanced(partitionId, objectId);

                var response = new ReadAdvancedResponse {
                    ObjectValue = objectValue.value,
                    Timestamp = objectValue.timestampCounter
                };

                return response;
            }
            catch (Exception) {
                /*Partition not founded */
                return new ReadAdvancedResponse {ObjectValue = "N/A"};
            }
        }

        public override Task<WriteAdvancedResponse> writeAdvanced(WriteAdvancedRequest request,
            ServerCallContext context) {
            _freezeUtilities.WaitForUnfreeze();
            delayFunction();
            return Task.FromResult(WriteAdvanced(request));
        }

        private WriteAdvancedResponse WriteAdvanced(WriteAdvancedRequest request) {
            Console.WriteLine("Received Write request. Id - " +
                              request.ObjectId + ". Value" + request.ObjectValue);
            try {
                var partitionId = request.PartitionId;
                var objectId = request.ObjectId;
                var objectValue = request.ObjectValue;

                var partition = _storage.GetPartitionOrThrowException(partitionId);
                if (partition.IsMaster) {
                    Console.WriteLine("I'm the master of this partition");
                    var timestamp = _storage.WriteAdvancedMaster(partitionId, objectId, objectValue, -1);
                    return new WriteAdvancedResponse {Timestamp = timestamp};
                }

                Console.WriteLine("I'm not the master of this partition");

                var url = _storage.GetMasterUrl(request.PartitionId);
                return new WriteAdvancedResponse {MasterServerUrl = new ServerUrlResponse {ServerUrl = url}};
            }
            catch (Exception e) {
                Console.WriteLine("I don't have the partition");

                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                Console.WriteLine("End of exception");
                return new WriteAdvancedResponse();
            }
        }

        public override Task<ListServerResponse> listServer(ListServerRequest request, ServerCallContext context) {
            //Not frozen
            delayFunction();

            Console.WriteLine("Received List Server Request: " + request.ToString());
            return Task.FromResult(_storage.ListServer());
        }

        public override Task<ListPartitionGlobalResponse> listPartitionGlobal(ListPartitionGlobalRequest request,
            ServerCallContext context) {
            _freezeUtilities.WaitForUnfreeze();
            Console.WriteLine("List PARTITION Global Request: " + request.ToString());
            return Task.FromResult(_storage.ListPartition(request.PartitionId));
        }

        public override Task<ServerUrlResponse> getServerUrl(ServerUrlRequest request, ServerCallContext context) {
            _freezeUtilities.WaitForUnfreeze();
            var serverId = request.ServerId;
            var serverUrl = _storage.GetServerOrThrowException(serverId);

            return Task.FromResult(new ServerUrlResponse {ServerUrl = serverUrl});
        }
    }
}