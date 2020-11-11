using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace ServerDomain{
    public class BaseSlaveServerService : BaseSlaveService.BaseSlaveServiceBase{
        private readonly BaseServerStorage _storage;

        public BaseSlaveServerService(BaseServerStorage storage){
            _storage = storage;
        }


        public override Task<LockResponse> lockServer(LockRequest request, ServerCallContext context){
            Console.WriteLine("Locking the server to write" +
                request.ObjectId + " in partition " +
                request.PartitionId
            );

            try
            {
                var partitionId = request.PartitionId;
                var objectId = request.ObjectId;
                var partition = _storage.Partitions[partitionId];

                BaseServerObjectInfo objectInfo;
                lock (partition.Objects) {
                    if (!partition.Objects.TryGetValue(objectId, out objectInfo))
                        partition.Objects.Add(objectId, objectInfo = new BaseServerObjectInfo("NA"));
                }
                objectInfo._lock.Set(); //unlock is called inside partition write slave
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine(e.StackTrace);
                return Task.FromResult(new LockResponse { Acknowledge = "NOK" });
            }
            return Task.FromResult(new LockResponse{Acknowledge = "Ok"});
        }

        public override Task<UnlockResponse> unlockServer(UnlockRequest request, ServerCallContext context){
            System.Console.WriteLine("Unlocking the server to write " +
                request.ObjectId + " in partition " +
                request.PartitionId + ". Object value: " + 
                request.ObjectValue
            );
            try
            {

                var partitionId = request.PartitionId;
                var objectId = request.ObjectId;
                var objectValue = request.ObjectValue;

                _storage.Partitions[partitionId].WriteSlave(objectId, objectValue);
                return Task.FromResult(new UnlockResponse { Acknowledge = "Ok" });
            }
            catch(Exception e)
            {
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine(e.StackTrace);
                return Task.FromResult(new UnlockResponse { Acknowledge = "NOK" });
            }
        }
    }
}