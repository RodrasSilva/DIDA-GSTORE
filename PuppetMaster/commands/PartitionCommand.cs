using System;
using System.Collections.Generic;
using System.Linq;
using PuppetMasterMain;

namespace DIDA_GSTORE.commands{
    public class PartitionCommand : ICommand{
        private const int NumberOfReplicasPosition = 0;
        private const int PartitionNamePosition = 1;

        private readonly int _numberOfReplicas;
        private readonly string _partitionName;
        private readonly string[] _servers;

        private PartitionCommand(int numberOfReplicas, string partitionName, string[] servers){
            _numberOfReplicas = numberOfReplicas;
            _partitionName = partitionName;
            _servers = servers;
        }

        public bool IsAsync => true;
        public bool IsSetup => true;


        public void Execute(PuppetMasterDomain puppetMaster){
            if (_numberOfReplicas != puppetMaster.ReplicationFactor)
                throw new Exception("ReplicationFactor: " + puppetMaster.ReplicationFactor +
                                    " does not match: " + _numberOfReplicas);

            var masterUrl = _servers[0];
            foreach (var serverId in _servers){
                if (!puppetMaster.partitionsPerServer.ContainsKey(serverId)){
                    Console.WriteLine(serverId);
                    puppetMaster.partitionsPerServer.Add(serverId, new List<PartitionInfo>());
                }

                var partitions = puppetMaster.partitionsPerServer[serverId];

                partitions.Add(new PartitionInfo{partitionId = _partitionName, masterUrl = masterUrl});
            }

            //throw new System.NotImplementedException();
        }

        public static ICommand ParseCommandLine(string[] arguments){
            if (arguments.Length < 3) throw new Exception("Invalid Partition Command ");

            var numberOfReplicas = int.Parse(arguments[NumberOfReplicasPosition]);
            var partitionName = arguments[PartitionNamePosition];
            var servers = arguments.Skip(2).ToArray();
            return new PartitionCommand(numberOfReplicas, partitionName, servers);
        }
    }
}