using System;
using System.Collections.Generic;

namespace ServerDomain {
    public class AdvancedServerStorage : IStorage {
        public AdvancedServerStorage() {
            Partitions = new Dictionary<string, AdvancedServerPartition>();
        }

        public Dictionary<string, AdvancedServerPartition> Partitions { get; }

        public string Read(string partitionId, string objKey) {
            return Partitions[partitionId].Read(objKey);
        }

        public string GetMasterUrl(string partitionId) {
            return Partitions[partitionId].GetMasterUrl();
        }

        public IPartition GetPartitionOrThrowException(string partitionId) {
            AdvancedServerPartition partition = null;
            if (Partitions.TryGetValue(partitionId, out partition)) return partition;

            throw new Exception("No such partition");
        }

        public ListServerResponse ListServer() {
            throw new NotImplementedException();
        }

        public ListGlobalResponse ListGlobal() {
            throw new NotImplementedException();
        }

        public void RegisterPartitionSlave(string partitionId, string slaveServerId, string slaveServerUrl) {
            throw new NotImplementedException();
        }
        public void RegisterPartitionMaster(string partitionId)
        {
            throw new NotImplementedException();
        }
        public void AddPartition(string partitionId, string masterUrl)
        {
            //lock (Partitions) {
            throw new NotImplementedException();
            //}
        }

        public void WriteMaster(string partitionId, string objKey, string objValue, int timestamp) {
            throw new NotImplementedException();
        }

        public void WriteSlave(string partitionId, string objKey, string objValue, int timestamp) {
            throw new NotImplementedException();
        }

        public bool IsPartitionMaster(string partitionId) {
            return Partitions[partitionId].IsMaster;
        }

        public void Write(string partitionId, string objKey, string objValue, int timestamp = -1) {
            Partitions[partitionId].Write(objKey, objValue, timestamp);
        }
    }
}