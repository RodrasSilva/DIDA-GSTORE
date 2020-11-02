using System.Threading.Tasks;
using Grpc.Core;

namespace ServerDomain {
    public class AdvancedSlaveServerService : AdvancedSlaveService.AdvancedSlaveServiceBase {
        private AdvancedServerStorage _storage;

        public AdvancedSlaveServerService(AdvancedServerStorage storage) {
            _storage = storage;
        }


        public override Task<WriteSlaveResponse> WriteSlave(WriteSlaveRequest request, ServerCallContext context) {
            int partitionId = request.PartitionId;
            string objectId = request.ObjectId;
            string objectValue = request.ObjectValue;
            int timestamp = request.Timestamp;
            //AdvancedServerPartition partition = _storage.GetPartitionOrThrowException(partitionId);
            //partition.WriteSlave(objectId, objectValue, timestamp);

            _storage.Write(partitionId, objectId, objectValue, timestamp);
            //if exception occurs its because:
            //Partition doesn't exist O.o - Ignore ? Should never occur because master server called this...
            return Task.FromResult(new WriteSlaveResponse());
        }
    }
}