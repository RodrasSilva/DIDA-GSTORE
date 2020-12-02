using System;
using System.Collections.Generic;
using System.Text;
using ServerDomain;
using static AdvancedServerObjectInfo;

namespace Server.utils {
    public interface IStorage {
        public IPartition GetPartitionOrThrowException(string partitionId);

        public string GetServerOrThrowException(string serverId);

        public string Read(string partitionId, string objKey);

        public ObjectVal ReadAdvanced(string partitionId, string objKey);
        public string GetMasterUrl(string partitionId);

        public void WriteMaster(string partitionId, string objKey, string objValue, int timestamp);
        public int WriteAdvancedMaster(string partitionId, string objKey, string objValue, int timestamp);
        public void WriteSlave(string partitionId, string objKey, string objValue, int timestamp);

        public List<PartitionMasters> GetPartitionMasters();

        public ListPartitionGlobalResponse ListPartition(string id);
        public ListServerResponse ListServer();
        public void RegisterPartitionSlave(string partitionId, string slaveServerId, string slaveServerUrl);

        public void RegisterPartitionMaster(string partitionId);
        public void AddPartition(string partitionId, string masterUrl);

        public void AddServer(string serverId, string url);

        /*
        public void ResetTimeout(string partitionId);

        public void SetSlaveTimeout(string partitionId);

        public bool AskVote(string partitionId);
        */
    }
}