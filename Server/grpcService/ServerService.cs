using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Server.storage;

namespace DIDA_GSTORE.ServerService {
    public class ServerService : DIDAService.DIDAServiceBase {
        private Storage _storage;

        public ServerService(Storage storage) {
            _storage = storage;
        }


        public override Task<WriteResponse> write(WriteRequest request, ServerCallContext context) {
            return Task.FromResult(Write(request));
        }

        public WriteResponse Write(WriteRequest request) {
            try {
                int partitionId = request.PartitionId;
                string objectId = request.ObjectId;
                string objectValue = request.ObjectValue;

                Partition partition = _storage.GetPartitionOrThrowException(partitionId);
                if (partition.IsMaster) {
                    _storage.Write(partitionId, objectId, objectValue);
                }
                else {
                    string url = _storage.GetMasterUrl(request.PartitionId);
                }
            }
            catch (Exception) //later to be named NotFound or smth
            { }

            return new WriteResponse();
        }

        public override Task<ReadResponse> read(ReadRequest request, ServerCallContext context) {
            return base.read(request, context);
        }

        public override Task<ListGlobalResponse> listGlobal(ListGlobalRequest request, ServerCallContext context) {
            return base.listGlobal(request, context);
        }

        public override Task<ListServerResponse> listServer(ListServerRequest request, ServerCallContext context) {
            return base.listServer(request, context);
        }
    }
}