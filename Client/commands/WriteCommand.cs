using System;
using Client;
using DIDA_GSTORE.grpcService;

namespace DIDA_GSTORE.commands
{
    public class WriteCommand : ICommand
    {
        private const int PartitionIdPosition = 0;
        private const int ObjectIdPosition = 1;
        private const int ObjectValuePosition = 2;

        private readonly string _partitionId;
        private readonly string _objectId;
        private readonly string _objectValue;

        private WriteCommand(string partitionId, string objectId, string objectValue)
        {
            _partitionId = partitionId;
            _objectId = objectId;
            _objectValue = objectValue;
        }

        public static WriteCommand ParseCommandLine(string[] arguments)
        {
            if (arguments.Length != 3)
            {
                throw new Exception("Invalid Write Command ");
            }

            var partitionId = arguments[PartitionIdPosition];
            var objectId = arguments[ObjectIdPosition];
            var objectValue = arguments[ObjectValuePosition];
            return new WriteCommand(partitionId, objectId, objectValue);
        }

        public void Execute(GrpcService grpcService)
        {
            WriteResponse response = grpcService.Write(_partitionId, _objectId, _objectValue);
            //TODO : Logic with response

        }
    }
}