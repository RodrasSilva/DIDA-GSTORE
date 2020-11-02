using System;
using System.Collections.Generic;
using System.Text;


namespace Server.baseServerStorage
{

    public class BaseServerStorage : Storage
    {
        public Dictionary<int, BaseServerPartition> Partitions { get; }

        public Partition GetPartitionOrThrowException(int partitionId)
        {
            BaseServerPartition partition = null;
            if (Partitions.TryGetValue(partitionId, out partition))
            {
                return partition;
            }

            throw new Exception("No such partition");
        }

        public BaseServerStorage()
        {
            Partitions = new Dictionary<int, BaseServerPartition>();
        }

        public string Read(int partitionId, string objKey)
        {
            return Partitions[partitionId].Read(objKey);
        }

        public bool IsPartitionMaster(int partitionId)
        {
            return Partitions[partitionId].IsMaster;
        }

        public string GetMasterUrl(int partitionId)
        {
            return Partitions[partitionId].GetMasterUrl();
        }

        public void Write(int partitionId, string objKey, string objValue, int timestamp = -1)
        {
            Partitions[partitionId].Write(objKey, objValue);
        }
    }
}
