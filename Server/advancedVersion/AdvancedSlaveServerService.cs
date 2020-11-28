using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace ServerDomain{
    public class AdvancedSlaveServerService : AdvancedSlaveService.AdvancedSlaveServiceBase{
        private readonly AdvancedServerStorage _storage;

        public AdvancedSlaveServerService(AdvancedServerStorage storage){
            _storage = storage;
        }


        public override Task<WriteSlaveResponse> WriteSlave(WriteSlaveRequest request, ServerCallContext context){
            var partitionId = request.PartitionId;
            var objectId = request.ObjectId;
            var objectValue = request.ObjectValue;
            var timestamp = request.Timestamp;
            //AdvancedServerPartition partition = _storage.GetPartitionOrThrowException(partitionId);
            //partition.WriteSlave(objectId, objectValue, timestamp);

            _storage.Write(partitionId, objectId, objectValue, timestamp);

            //if exception occurs its because:
            //Partition doesn't exist O.o - Ignore ? Should never occur because master server called this...
            return Task.FromResult(new WriteSlaveResponse());
        }
    }
}