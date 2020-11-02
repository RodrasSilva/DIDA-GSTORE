using System.Collections.Generic;
using System.Linq;

public class BaseServerPartition : IPartition {
    public class SlaveInfo {
        public int ServerId { get; }
        public BaseSlaveService.BaseSlaveServiceClient SlaveChannel { get; }

        public SlaveInfo(int serverId, BaseSlaveService.BaseSlaveServiceClient slaveChannel) {
            ServerId = serverId;
            SlaveChannel = slaveChannel;
        }
    }

    public bool IsMaster { get; set; }
    private int _partitionId;
    private List<SlaveInfo> slaveServiceClients;
    private string _masterUrl;

    public Dictionary<string, BaseServerObjectInfo> Objects { get; }

    public BaseServerPartition(int partitionId, string masterUrl) {
        _partitionId = partitionId;
        _masterUrl = masterUrl;
        Objects = new Dictionary<string, BaseServerObjectInfo>();
        slaveServiceClients = new List<SlaveInfo>();
    }

    public string GetMasterUrl() {
        return _masterUrl;
    }

    public string Read(string objKey) {
        BaseServerObjectInfo objectInfo = Objects[objKey];
        objectInfo._lock.AcquireReaderLock(0);
        try {
            return objectInfo.Read();
        }
        finally {
            objectInfo._lock.ReleaseReaderLock();
        }
    }

    public void WriteMaster(string objKey, string objValue) {
        LockRequest lockRequest = new LockRequest();
        UnlockRequest unlockRequest = new UnlockRequest {
            ObjectId = objKey,
            ObjectValue = objValue
        };
        //Very important - order slaves

        IEnumerable<SlaveInfo> orderedSlaves = slaveServiceClients.OrderBy((s) => s.ServerId);

        BaseServerObjectInfo objectInfo;
        lock (Objects) {
            if (!Objects.TryGetValue(objKey, out objectInfo)) {
                Objects.Add(objKey, objectInfo = new BaseServerObjectInfo("NA"));
            }
        }

        objectInfo._lock.AcquireWriterLock(0);
        objectInfo.Write(objValue);

        foreach (SlaveInfo slave in orderedSlaves) {
            slave.SlaveChannel.lockServer(lockRequest);
        }

        //storage.Objects.Add(request.ObjectId, request.ObjectValue);

        foreach (SlaveInfo slave in orderedSlaves) {
            slave.SlaveChannel.unlockServer(unlockRequest);
        }

        objectInfo._lock.ReleaseWriterLock();
    }

    public void WriteSlave(string objKey, string objValue) {
        //ignore, in base version write slave is done by lock and unlock
    }
}