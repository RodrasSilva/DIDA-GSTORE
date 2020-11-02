using System.Collections.Generic;

public class BaseServerPartition : IPartition
{
    public bool IsMaster { get; set; }
    private int _partitionId;
    private List<BaseSlaveService.BaseSlaveServiceClient> slaveServiceClients;
    private string _masterUrl;

    public Dictionary<string, BaseServerObjectInfo> Objects { get; }

    public BaseServerPartition(int partitionId, string masterUrl)
    {
        _partitionId = partitionId;
        _masterUrl = masterUrl;
        Objects = new Dictionary<string, BaseServerObjectInfo>();
        slaveServiceClients = new List<BaseSlaveService.BaseSlaveServiceClient>();
    }

    public string GetMasterUrl()
    {
        return _masterUrl;
    }

    public string Read(string objKey)
    {
        lock (this)
        {
            return Objects[objKey].Read();
        }
    }

    public void Write(string objKey, string objValue)
    {
        if (IsMaster) WriteMaster(objKey, objValue);
        WriteSlave(objKey, objValue);
    }

    public void WriteMaster(string objKey, string objValue)
    {
        foreach()
    }

    public void WriteSlave(string objKey, string objValue)
    {
    }
}