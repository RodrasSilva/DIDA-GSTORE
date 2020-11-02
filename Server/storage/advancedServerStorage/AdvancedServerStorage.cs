using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Server.advancedServerStorage
{
  

    public class AdvancedServerStorage : Storage {
        public Dictionary<int, AdvancedServerPartition> Partitions { get; }

        public AdvancedServerStorage()
        {
            Partitions = new Dictionary<int, AdvancedServerPartition>();
        }

        public string Read(int partitionId, string objKey) {
            return Partitions[partitionId].Read(objKey);
        }

        public bool IsPartitionMaster(int partitionId) {
            return Partitions[partitionId].IsMaster;
        }

        public string GetMasterUrl(int partitionId) {
            return Partitions[partitionId].GetMasterUrl();
        }

        public void Write(int partitionId, string objKey, string objValue, int timestamp = -1) {
            Partitions[partitionId].Write(objKey, objValue, timestamp);
        }

        public Partition GetPartitionOrThrowException(int partitionId){
            AdvancedServerPartition partition = null;
            if (Partitions.TryGetValue(partitionId, out partition))
            {
                return partition;
            }

            throw new Exception("No such partition");
        }

    }   
}