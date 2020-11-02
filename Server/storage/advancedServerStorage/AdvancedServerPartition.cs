using System.Collections.Generic;

public class AdvancedServerPartition : IPartition {
    public bool IsMaster { get; set; }
    private int _partitionId;
    private List<AdvancedSlaveService.AdvancedSlaveServiceClient> slaveServiceClients;
    private string _masterUrl;

    public Dictionary<string, AdvancedServerObjectInfo> Objects { get; }

    public AdvancedServerPartition(int partitionId, string masterUrl) {
        _partitionId = partitionId;
        _masterUrl = masterUrl;
        Objects = new Dictionary<string, AdvancedServerObjectInfo>();
        slaveServiceClients = new List<AdvancedSlaveService.AdvancedSlaveServiceClient>();
    }

    public string GetMasterUrl() {
        return _masterUrl; //temporary solution
    }

    public string Read(string objKey) {
        return Objects[objKey].Read();
    }

    public void Write(string objKey, string objValue, int timestamp = -1) {
        if (IsMaster) WriteMaster(objKey, objValue);
        WriteSlave(objKey, objValue, timestamp);
    }

    public void WriteMaster(string objKey, string objValue) {
        int timeStamp = Objects[objKey].WriteNext(objValue);
        WriteSlaveRequest request = new WriteSlaveRequest {
            PartitionId = _partitionId,
            ObjectId = objKey,
            ObjectValue = objValue,
            Timestamp = timeStamp
        };
        foreach (AdvancedSlaveService.AdvancedSlaveServiceClient slave in slaveServiceClients) {
            slave.WriteSlaveAsync(request);
        }
    }

    public void WriteSlave(string objKey, string objValue, int timestamp) {
        Objects[objKey].Write(objValue, timestamp);
    }
}