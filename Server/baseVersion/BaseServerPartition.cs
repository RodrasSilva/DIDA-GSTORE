using System;
using System.Collections.Generic;
using System.Linq;
using Server.utils;

public class BaseServerPartition : IPartition{
    private readonly string _masterUrl;
    private string _id;

    public BaseServerPartition(string id, string masterUrl){
        _id = id;
        _masterUrl = masterUrl;
        Objects = new Dictionary<string, BaseServerObjectInfo>();
        SlaveServers = new List<SlaveInfo>();
        IsMaster = false;
    }

    public Dictionary<string, BaseServerObjectInfo> Objects{ get; }
    public List<SlaveInfo> SlaveServers{ get; }

    public bool IsMaster{ get; set; }

    public string GetMasterUrl(){
        return _masterUrl;
    }

    public string Read(string objKey){
        var objectInfo = Objects[objKey];
        objectInfo._lock.Set();
        try{
            return objectInfo.Read();
        }
        finally{
            objectInfo._lock.Reset();
        }
    }

    public void WriteMaster(string objKey, string objValue){
        var lockRequest = new LockRequest()
        {
            PartitionId = _id,
            ObjectId = objKey,
        };
        var unlockRequest = new UnlockRequest {
            PartitionId = _id,
            ObjectId = objKey,
            ObjectValue = objValue
        };
        //Very important - order slaves

        IEnumerable<SlaveInfo> orderedSlaves = SlaveServers.OrderBy(s => s.ServerId);

        BaseServerObjectInfo objectInfo;
        lock (Objects){
            if (!Objects.TryGetValue(objKey, out objectInfo))
            {
                Objects.Add(objKey, objectInfo = new BaseServerObjectInfo("NA"));
            }
        }

        objectInfo._lock.Set();
        objectInfo.Write(objValue);

        foreach (var slave in orderedSlaves) {
            try
            {
                slave.SlaveChannel.lockServer(lockRequest);
            }catch(Exception)
            {
                Console.WriteLine($"Error locking partition {_id} slave {slave.ServerId}");
            }
           
        }

        foreach (var slave in orderedSlaves)
        {
            try
            {
                slave.SlaveChannel.unlockServer(unlockRequest);
            }
            catch (Exception)
            {
                Console.WriteLine($"Error unlocking partition {_id} slave {slave.ServerId}");
            }

        }
        objectInfo._lock.Reset();
    }

    public void WriteSlave(string objKey, string objectValue){
        //lock was acquired in lockServer
        var objectInfo = Objects[objKey];
        //write slave 
        objectInfo.Write(objectValue);
        objectInfo._lock.Reset();
        //unlock object

        //ignore, in base version write slave is done by lock and unlock
    }

    public class SlaveInfo{
        public SlaveInfo(string serverId, BaseSlaveService.BaseSlaveServiceClient slaveChannel){
            ServerId = serverId;
            SlaveChannel = slaveChannel;
        }

        public string ServerId{ get; }
        public BaseSlaveService.BaseSlaveServiceClient SlaveChannel{ get; }
    }
}