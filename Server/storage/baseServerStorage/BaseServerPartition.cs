using System.Collections.Generic;

public class BaseServerPartition : Partition
{
    public bool IsMaster { get; set; }
    private int _partitionId;
    private List<BaseSlaveServiceClient> slaveServiceClients;
    private string _masterUrl;

    public Dictionary<string, BaseServerObjectInfo> Objects { get; }

    public BaseServerPartition(int partitionId, string masterUrl)
    {
        _partitionId = partitionId;
        _masterUrl = masterUrl;
        Objects = new Dictionary<string, BaseServerObjectInfo>();
        slaveServiceClients = new List<BaseSlaveServiceClient>();
    }

    public string GetMasterUrl()
    {
        
    }

    public string Read(string objKey)
    {
        
    }

    public void Write(string objKey, string objValue)
    {
        if (IsMaster) WriteMaster(objKey, objValue);
        WriteSlave(objKey, objValue);
    }

    public void WriteMaster(string objKey, string objValue)
    {
     
    }

    public void WriteSlave(string objKey, string objValue)
    {
        
    }


}