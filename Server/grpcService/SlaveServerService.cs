using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Server.storage;

namespace DIDA_GSTORE.SlaveServerService {
    public class SlaveServerServiceServer : SlaveService.SlaveServiceBase {
        private Storage _storage;

        public SlaveServerServiceServer(Storage storage) {
            _storage = storage;
        }


        public override Task<WriteSlaveResponse> WriteSlave(WriteSlaveRequest request, ServerCallContext context) {
            int partitionId = request.PartitionId;
            string objectId = request.ObjectId;
            string objectValue = request.ObjectValue;
            int timestamp = request.Timestamp;
            Partition partition = _storage.GetPartitionOrThrowException(partitionId);
            partition.WriteSlave(objectId, objectValue, timestamp);
            //if exception occurs its because:
            //Partition doesn't exist O.o - Ignore ? Should never occur because master server called this...
            return Task.FromResult(new WriteSlaveResponse());
        }
    }
}