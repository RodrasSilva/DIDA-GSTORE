using System.Threading.Tasks;

namespace ServerDomain {
    public class BaseSlaveServerService : BaseSlaveService.BaseSlaveServiceBase {
        private BaseServerStorage _storage;

        public BaseSlaveServerService(BaseServerStorage storage) {
            _storage = storage;
        }


        public override Task<LockResponse> lockServer(LockRequest request, Grpc.Core.ServerCallContext context) {
            int partitionId = request.PartitionId;
            string objectId = request.ObjectId;
            BaseServerObjectInfo objectInfo = _storage.Partitions[partitionId].Objects[objectId];
            objectInfo._lock.AcquireWriterLock(0);
            return Task.FromResult(new LockResponse {Acknowledge = "Ok"});
        }

        public override Task<UnlockResponse> unlockServer(UnlockRequest request, Grpc.Core.ServerCallContext context) {
            int partitionId = request.PartitionId;
            string objectId = request.ObjectId;
            string objectValue = request.ObjectValue;
            BaseServerObjectInfo objectInfo = _storage.Partitions[partitionId].Objects[objectId];
            //write slave 
            objectInfo.Write(objectValue);
            objectInfo._lock.ReleaseWriterLock();
            //unlock object
            return Task.FromResult(new UnlockResponse {Acknowledge = "Ok"});
        }
    }
}