using System;
using System.Linq;
using PuppetMasterMain;

namespace DIDA_GSTORE.commands {
    public class PartitionCommand : ICommand {
        private const int NumberOfReplicasPosition = 0;
        private const int PartitionNamePosition = 1;

        private readonly int _numberOfReplicas;
        private readonly int _partitionName;
        private readonly string[] _servers;

        private PartitionCommand(int numberOfReplicas, int partitionName, string[] servers) {
            _partitionName = partitionName;
            _servers = servers;
        }

        public bool IsAsync => true;
        public bool IsSetup => true;


        public void Execute(PuppetMasterDomain puppetMaster) {
            if (_numberOfReplicas != puppetMaster.ReplicationFactor)
                throw new Exception("ReplicationFactor does not match");

            int masterId = _servers[0];
            foreach (int serverId in _servers) {
                List<PartitionInfo> partitions = puppetMaster.partitionsPerServer[serverId];
                if (partitions == null) {
                    puppetMaster.partitionsPerServer[serverId] = new List<PartitionInfo>();
                    partitions = puppetMaster.partitionsPerServer[serverId];
                }

                partitions.Add(new PartitionInfo {_partitionName, masterId});
            }

            //throw new System.NotImplementedException();
        }

        public static ICommand ParseCommandLine(string[] arguments) {
            if (arguments.Length < 3) throw new Exception("Invalid Partition Command ");

            var numberOfReplicas = int.Parse(arguments[NumberOfReplicasPosition]);
            var partitionName = int.Parse(arguments[PartitionNamePosition]);
            var servers = arguments.Skip(2).ToArray();
            return new PartitionCommand(numberOfReplicas, partitionName, servers);
        }
    }
}