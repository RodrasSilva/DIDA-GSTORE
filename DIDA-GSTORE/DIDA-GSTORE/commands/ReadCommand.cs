using System;
using Client;
using DIDA_GSTORE.grpcService;

namespace DIDA_GSTORE.commands
{
    public class ReadCommand : ICommand
    {
        private const int PartitionIdPosition = 0;
        private const int ObjectIdPosition = 1;
        private const int ServerIdPosition = 2;

        private readonly string _partitionId;
        private readonly string _objectId;
        private readonly string _serverId;

        private ReadCommand(string partitionId, string objectId, string serverId)
        {
            _partitionId = partitionId;
            _objectId = objectId;
            _serverId = serverId;
        }

        public static ReadCommand ParseCommandLine(string[] arguments)
        {
            if (arguments.Length != 3)
            {
                throw new Exception("Invalid Read Command ");
            }

            var partitionId = arguments[PartitionIdPosition];
            var objectId = arguments[ObjectIdPosition];
            var serverId = arguments[ServerIdPosition];
            return new ReadCommand(partitionId, objectId, serverId);
        }

        public void Execute(GrpcService grpcService)
        {
            ReadResponse response = grpcService.Read(_partitionId, _objectId, _serverId);
            //TODO :  Logic with response
        }
    }
}