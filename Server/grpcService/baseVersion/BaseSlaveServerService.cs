using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;

namespace ServerDomain {
    public class BaseSlaveServerService : BaseSlaveService.BaseSlaveServiceBase {
        private readonly BaseServerStorage _storage;

        public BaseSlaveServerService(BaseServerStorage storage) {
            _storage = storage;
        }


        public override Task<LockResponse> lockServer(LockRequest request, ServerCallContext context) {
            var partitionId = request.PartitionId;
            var objectId = request.ObjectId;
            var partition = _storage.Partitions[partitionId];

            BaseServerObjectInfo objectInfo;

            lock (partition.Objects) {
                if (!partition.Objects.TryGetValue(objectId, out objectInfo))
                    partition.Objects.Add(objectId, objectInfo = new BaseServerObjectInfo("NA"));

                objectInfo._lock.AcquireWriterLock(0); //unlock is called inside partition write slave
            }

            return Task.FromResult(new LockResponse {Acknowledge = "Ok"});
        }

        public override Task<UnlockResponse> unlockServer(UnlockRequest request, ServerCallContext context) {
            var partitionId = request.PartitionId;
            var objectId = request.ObjectId;
            var objectValue = request.ObjectValue;

            _storage.Partitions[partitionId].WriteSlave(objectId, objectValue);
            return Task.FromResult(new UnlockResponse {Acknowledge = "Ok"});
        }
    }
}