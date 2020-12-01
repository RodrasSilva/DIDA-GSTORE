using System;
using System.Linq;
using DIDA_GSTORE.grpcService;

namespace DIDA_GSTORE.commands{
    public class WriteCommand : ICommand{
        private const int PartitionIdPosition = 0;
        private const int ObjectIdPosition = 1;
        private readonly string _objectId;
        private readonly string _objectValue;
        private readonly string _partitionId;

        private WriteCommand(string partitionId, string objectId, string objectValue){
            _partitionId = partitionId;
            _objectId = objectId;
            _objectValue = objectValue;
        }

        public void Execute(GrpcService grpcService){

            if (grpcService.UseBaseVersion)
            {
                Console.WriteLine($"Client executing write command with {_partitionId} {_objectId} {_objectValue}");
                grpcService.Write(_partitionId, _objectId, _objectValue);
            }
            else
            {
                Console.WriteLine($"Client executing write command with {_partitionId} {_objectId} {_objectValue}");
                grpcService.WriteAdvanced(_partitionId, _objectId, _objectValue);
            }
        }

        public static WriteCommand ParseCommandLine(string[] arguments){
            if (arguments.Length < 3) throw new Exception("Invalid Write Command ");
            var partitionId = arguments[PartitionIdPosition];
            var objectId = arguments[ObjectIdPosition];
            var objectValueArguments = arguments.Skip(2);
            var objectValue = string.Join(" ", objectValueArguments);
            objectValue = objectValue.Substring(0, objectValue.Length - 1).Substring(1);
            return new WriteCommand(partitionId, objectId, objectValue);
        }
    }
}