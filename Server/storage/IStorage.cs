﻿public interface IStorage {
    public IPartition GetPartitionOrThrowException(string partitionId);

    public string Read(string partitionId, string objKey);

    // public bool IsPartitionMaster(string partitionId);

    public string GetMasterUrl(string partitionId);

    public void WriteMaster(string partitionId, string objKey, string objValue, int timestamp);
    public void WriteSlave(string partitionId, string objKey, string objValue, int timestamp);

    public ListServerResponse ListServer();
    public ListGlobalResponse ListGlobal();
    void RegisterPartitionSlave(string partitionId, string slaveServerId, string slaveServerUrl);
}