using System;
using System.Collections.Generic;
using Server.utils;
using static AdvancedServerObjectInfo;

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

    public ObjectVal ReadAdvanced(string objKey, 
        string clientObjectValue, int clientTimestamp)
    {
        var objectInfo = Objects[objKey];
        objectInfo._lock.Set();
        try
        {
            return objectInfo.Read(clientObjectValue, clientTimestamp);
        }
        finally
        {
            objectInfo._lock.Reset();
        }
    }

    public void Write(string objKey, string objValue, int timestamp = -1){
        if (IsMaster) WriteMaster(objKey, objValue);
        WriteSlave(objKey, objValue, timestamp);
    }

    public void WriteMaster(string objKey, string objValue)
    {
        AdvancedServerObjectInfo objectInfo;

        lock (Objects)
        {
            if (!Objects.TryGetValue(objKey, out objectInfo))
            {
                Objects.Add(objKey, objectInfo = new AdvancedServerObjectInfo("NA"));
            }
        }
        var timeStamp = Objects[objKey].WriteNext(objValue);
        var request = new WriteSlaveRequest {
            PartitionId = _id,
            ObjectId = objKey,
            ObjectValue = objValue,
            Timestamp = timeStamp
        };

        List<ServerInfo> toRemove = new List<ServerInfo>();
        foreach (var slave in Servers)
        {
            try
            {
                slave.ServerChannel.WriteSlaveAsync(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("ONE OF THE SERVERS I WAS TALKING TO (" + slave.ServerId +
                    ") OFFICIALLY DIED");
                toRemove.Add(slave);
            }
        }

        foreach(var slave in toRemove)
        {
            Servers.Remove(slave);
        }
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