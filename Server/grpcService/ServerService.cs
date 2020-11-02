using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace DIDA_GSTORE.ServerService {
    public class ServerService : DIDAService.DIDAServiceBase {
        private IStorage _storage;

        public ServerService(IStorage storage) {
            _storage = storage;
        }


        public override Task<WriteResponse> write(WriteRequest request, ServerCallContext context) {
            ServerDomain.Server.DelayMessage();
            return Task.FromResult(Write(request));
        }

        public WriteResponse Write(WriteRequest request) {
            ServerDomain.Server.DelayMessage();
            try {
                int partitionId = request.PartitionId;
                string objectId = request.ObjectId;
                string objectValue = request.ObjectValue;

                IPartition partition = _storage.GetPartitionOrThrowException(partitionId);
                if (partition.IsMaster) {
                    _storage.WriteMaster(partitionId, objectId, objectValue, -1);
                    return new WriteResponse {ResponseMessage = "OK"};
                }
                else {
                    string url = _storage.GetMasterUrl(request.PartitionId);
                    return new WriteResponse {MasterServerUrl = new ServerUrlResponse {ServerUrl = url}};
                }
            }
            catch (Exception) //later to be named NotFound or smth
            {
                return new WriteResponse { };
            }
        }

        public override Task<ReadResponse> read(ReadRequest request, ServerCallContext context) {
            ServerDomain.Server.DelayMessage();
            return Task.FromResult(Read(request));
        }


        public ReadResponse Read(ReadRequest request) {
            ServerDomain.Server.DelayMessage();
            try {
                int partitionId = request.PartitionId;
                string objectId = request.ObjectId;
                IPartition partition = _storage.GetPartitionOrThrowException(partitionId);
                string objectValue = _storage.Read(partitionId, objectId);
                ReadResponse response = new ReadResponse {ObjectValue = objectValue};
                return response;
            }
            catch (Exception) {
                /*Partition not founded */
                return new ReadResponse {ObjectValue = "-1"};
            }
        }

        public override Task<ListServerResponse> listServer(ListServerRequest request, ServerCallContext context) {
            ServerDomain.Server.DelayMessage();
            return Task.FromResult(_storage.ListServer());
        }

        public override Task<ListGlobalResponse> listGlobal(ListGlobalRequest request, ServerCallContext context) {
            ServerDomain.Server.DelayMessage();
            return Task.FromResult(_storage.ListGlobal());
        }

        public override Task<ServerUrlResponse> getServerUrl(ServerUrlRequest request, ServerCallContext context) {
            return base.getServerUrl(request, context); // TODO : Implement
        }
    }
}