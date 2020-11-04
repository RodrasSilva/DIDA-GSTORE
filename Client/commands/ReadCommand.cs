using System;
using DIDA_GSTORE.grpcService;

namespace DIDA_GSTORE.commands{
    public class ReadCommand : ICommand{
        private const int PartitionIdPosition = 0;
        private const int ObjectIdPosition = 1;
        private const int ServerIdPosition = 2;
        private readonly string _objectId;

        private readonly string _partitionId;
        private readonly string _serverId;

        private ReadCommand(string partitionId, string objectId, string serverId){
            _partitionId = partitionId;
            _objectId = objectId;
            _serverId = serverId;
        }

        public void Execute(GrpcService grpcService){
            var response = grpcService.Read(_partitionId, _objectId, _serverId);
            Console.WriteLine($"Read from partition {_partitionId} object {_objectId} returned {response}");
        }

        public static ReadCommand ParseCommandLine(string[] arguments){
            if (!(arguments.Length != 2 || arguments.Length != 3)) throw new Exception("Invalid Read Command ");

            var partitionId = arguments[PartitionIdPosition];
            var objectId = arguments[ObjectIdPosition];
            var serverId = arguments.Length == 2 ? "-1" : arguments[ServerIdPosition];
            return new ReadCommand(partitionId, objectId, serverId);
        }
    }
}