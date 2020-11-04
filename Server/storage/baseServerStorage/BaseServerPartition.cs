using System.Collections.Generic;
using System.Linq;

public class BaseServerPartition : IPartition {
    private readonly string _masterUrl;

    public BaseServerPartition(string masterUrl) {
        _masterUrl = masterUrl;
        Objects = new Dictionary<string, BaseServerObjectInfo>();
        SlaveServers = new List<SlaveInfo>();
        IsMaster = false;
    }

    public Dictionary<string, BaseServerObjectInfo> Objects { get; }
    public List<SlaveInfo> SlaveServers { get; }

    public bool IsMaster { get; set; }

    public string GetMasterUrl() {
        return _masterUrl;
    }

    public string Read(string objKey) {
        var objectInfo = Objects[objKey];
        objectInfo._lock.AcquireReaderLock(0);
        try {
            return objectInfo.Read();
        }
        finally {
            objectInfo._lock.ReleaseReaderLock();
        }
    }

    public void WriteMaster(string objKey, string objValue) {
        var lockRequest = new LockRequest();
        var unlockRequest = new UnlockRequest {
            ObjectId = objKey,
            ObjectValue = objValue
        };
        //Very important - order slaves

        IEnumerable<SlaveInfo> orderedSlaves = SlaveServers.OrderBy(s => s.ServerId);

        BaseServerObjectInfo objectInfo;
        lock (Objects) {
            if (!Objects.TryGetValue(objKey, out objectInfo))
                Objects.Add(objKey, objectInfo = new BaseServerObjectInfo("NA"));
        }

        objectInfo._lock.AcquireWriterLock(0);
        objectInfo.Write(objValue);

        foreach (var slave in orderedSlaves) slave.SlaveChannel.lockServer(lockRequest);

        //storage.Objects.Add(request.ObjectId, request.ObjectValue);

        foreach (var slave in orderedSlaves) slave.SlaveChannel.unlockServer(unlockRequest);

        objectInfo._lock.ReleaseWriterLock();
    }

    public void WriteSlave(string objKey, string objectValue) {
        //lock was acquired in lockServer
        var objectInfo = Objects[objKey];
        //write slave 
        objectInfo.Write(objectValue);
        objectInfo._lock.ReleaseWriterLock();
        //unlock object

        //ignore, in base version write slave is done by lock and unlock
    }

    public class SlaveInfo {
        public SlaveInfo(string serverId, BaseSlaveService.BaseSlaveServiceClient slaveChannel) {
            ServerId = serverId;
            SlaveChannel = slaveChannel;
        }

        public string ServerId { get; }
        public BaseSlaveService.BaseSlaveServiceClient SlaveChannel { get; }
    }
}