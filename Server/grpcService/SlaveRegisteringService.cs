using System;
using System.Threading.Tasks;
using Grpc.Core;
using Server.utils;

namespace Server.grpcService{
    public class SlaveRegisteringService : RegisterSlaveToMasterService.RegisterSlaveToMasterServiceBase{
        private readonly IStorage _storage;

        public SlaveRegisteringService(IStorage storage){
            _storage = storage;
        }

        public override Task<RegisterResponse> registerAsSlave(RegisterRequest request, ServerCallContext context){
            Console.WriteLine("Received Slave Registration Request: " + request.ToString());
            var partitionId = request.PartitionId;
            var slaveServerId = request.ServerId;
            var slaveServerUrl = request.Url;
            _storage.RegisterPartitionSlave(partitionId, slaveServerId, slaveServerUrl);
            return Task.FromResult(new RegisterResponse{Ack = true});
        }
    }
}