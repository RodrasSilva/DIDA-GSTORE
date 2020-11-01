using PuppetMasterMain;
using System;
using System.Linq;

namespace DIDA_GSTORE.commands {
    public class PartitionCommand : ICommand {
        public bool IsAsync => true;
        private const int NumberOfReplicasPosition = 0;
        private const int PartitionNamePosition = 1;

        private readonly int _numberOfReplicas;
        private readonly string _partitionName;
        private readonly string[] _servers;

        private PartitionCommand(int numberOfReplicas, string partitionName, string[] servers) {
            _numberOfReplicas = numberOfReplicas;
            _partitionName = partitionName;
            _servers = servers;
        }


        public void Execute(PuppetMasterDomain puppetMaster) {
            throw new System.NotImplementedException();
        }

        public static ICommand ParseCommandLine(string[] arguments) {
            if (arguments.Length < 3) {
                throw new Exception("Invalid Partition Command ");
            }

            var numberOfReplicas = int.Parse(arguments[NumberOfReplicasPosition]);
            var partitionName = arguments[PartitionNamePosition];
            var servers = arguments.Skip(2).ToArray();
            return new PartitionCommand(numberOfReplicas, partitionName, servers);
        }
    }
}