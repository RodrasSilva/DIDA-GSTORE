using System;
using System.Collections.Generic;
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

        public override Task<HeartbeatResponse> Heartbeat(HeartbeatRequest request, ServerCallContext context) {
            Console.WriteLine("[RECEIVED] Heartbeat. Heartbeat for Partition: " + request.PartitionId);
            _storage.ResetTimeout(request.PartitionId);
            return Task.FromResult(new HeartbeatResponse());
        }

        public override Task<VoteResponse> AskVote(VoteRequest request, ServerCallContext context)
        {
            Console.WriteLine("Finished Asking for Votes for Partition :" + request.PartitionId);
            bool res = _storage.AskVote(request.PartitionId);

            return Task.FromResult(new VoteResponse { Res = res });
            //return base.AskVote(request, context);
        }

        public override Task<InformLeaderResponse> InformLeader(InformLeaderRequest request, ServerCallContext context)
        {
            Console.WriteLine("[Not Mine:" + request.PartitionId + "] THERE IS A NEW LEADER: " + request.MasterUrl);

            _storage.InformLeader(request.PartitionId, request.MasterUrl);

            return Task.FromResult(new InformLeaderResponse {});
        }

        public override Task<InformLeaderPartitionResponse> InformLeaderPartition(InformLeaderPartitionRequest request, ServerCallContext context)
        {
            Console.WriteLine("[MyPartition:" + request.PartitionId + "] THERE IS A NEW LEADER: " + request.NewMasterUrl);
            List<ObjectInfo> objectInfos = _storage.InformLeaderPartition(request.PartitionId, request.NewMasterUrl, new List<ObjectInfo>(request.ObjectInfo));

            return Task.FromResult(new InformLeaderPartitionResponse { ObjectInfo = { objectInfos } });
        }

        public override Task<FinishLeaderTransitionResponse> FinishLeaderTransition(FinishLeaderTransitionRequest request, ServerCallContext context)
        {
            Console.WriteLine("Finished Leader transition.");

            foreach(var objInfo in request.ObjectInfo)
            {
                _storage.Write(request.PartitionId, objInfo.ObjectId,
                    objInfo.ObjectValue, objInfo.Timestamp);
            }

            return Task.FromResult(new FinishLeaderTransitionResponse {});
        }
    }
}