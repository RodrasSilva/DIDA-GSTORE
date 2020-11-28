using System.Collections.Generic;
using Server.utils;

public class AdvancedServerPartition : IPartition{
    private readonly string _masterUrl;
    private string _id;
    public List<ServerInfo> Servers { get; }

    public Dictionary<string, AdvancedServerObjectInfo> Objects { get; }

    public AdvancedServerPartition(string id, string masterUrl){
        _id = id;
        _masterUrl = masterUrl;
        Objects = new Dictionary<string, AdvancedServerObjectInfo>();
        Servers = new List<ServerInfo>();
        IsMaster = false;
    }
    public bool IsMaster{ get; set; }

    public string GetMasterUrl(){
        return _masterUrl;
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
            PartitionId = _id,
            ObjectId = objKey,
            ObjectValue = objValue,
            Timestamp = timeStamp
        };
        foreach (var slave in Servers) slave.ServerChannel.WriteSlaveAsync(request);
    }

    public void WriteSlave(string objKey, string objValue, int timestamp){
        Objects[objKey].Write(objValue, timestamp);
    }


    public class ServerInfo
    {
        public ServerInfo(string serverId, AdvancedSlaveService.AdvancedSlaveServiceClient serverChannel)
        {
            ServerId = serverId;
            ServerChannel = serverChannel;
        }

        public string ServerId { get; }
        public AdvancedSlaveService.AdvancedSlaveServiceClient ServerChannel { get; }
    }
}