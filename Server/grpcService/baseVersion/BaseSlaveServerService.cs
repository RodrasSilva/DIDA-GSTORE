using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;

namespace ServerDomain {
    public class BaseSlaveServerService : BaseSlaveService.BaseSlaveServiceBase {
        private readonly BaseServerStorage _storage;

        public BaseSlaveServerService(string serverId, string serverUrl, string[] masterPartitionsInfo,
            BaseServerStorage storage) {
            _storage = storage;
            for (var i = 0; i < masterPartitionsInfo.Length; i += 2) {
                var partitionId = masterPartitionsInfo[i];
                var partitionMasterUrl = masterPartitionsInfo[i + 1];
                if (partitionMasterUrl.Equals(serverUrl)) continue; // Important, cannot be slave and master to the same partition
                try {
                    var request = new RegisterRequest {ServerId = serverId, Url = serverUrl, PartitionId = partitionId};
                    Console.WriteLine($"Registering as slave to partition {partitionId}");
                    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                    var channel = GrpcChannel.ForAddress(partitionMasterUrl);
                    var client =
                        new RegisterSlaveToMasterService.RegisterSlaveToMasterServiceClient(channel);
                    client.register(request);
                }
                catch (Exception) {
                    Console.WriteLine($"Error registering as slave to partition {partitionId}");
                }
            }
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