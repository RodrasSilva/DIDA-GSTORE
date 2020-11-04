using System.Collections.Generic;

public class AdvancedServerPartition : IPartition{
    private readonly string _masterUrl;
    private readonly string _partitionId;
    private readonly List<AdvancedSlaveService.AdvancedSlaveServiceClient> slaveServiceClients;

    public AdvancedServerPartition(string partitionId, string masterUrl){
        _partitionId = partitionId;
        _masterUrl = masterUrl;
        Objects = new Dictionary<string, AdvancedServerObjectInfo>();
        slaveServiceClients = new List<AdvancedSlaveService.AdvancedSlaveServiceClient>();
    }

    public Dictionary<string, AdvancedServerObjectInfo> Objects{ get; }
    public bool IsMaster{ get; set; }

    public string GetMasterUrl(){
        return _masterUrl; //temporary solution
    }

    public string Read(string objKey){
        return Objects[objKey].Read();
    }

    public void Write(string objKey, string objValue, int timestamp = -1){
        if (IsMaster) WriteMaster(objKey, objValue);
        WriteSlave(objKey, objValue, timestamp);
    }

    public void WriteMaster(string objKey, string objValue){
        var timeStamp = Objects[objKey].WriteNext(objValue);
        var request = new WriteSlaveRequest {
            PartitionId = _partitionId,
            ObjectId = objKey,
            ObjectValue = objValue,
            Timestamp = timeStamp
        };
        foreach (var slave in slaveServiceClients) slave.WriteSlaveAsync(request);
    }

    public void WriteSlave(string objKey, string objValue, int timestamp){
        Objects[objKey].Write(objValue, timestamp);
    }
}