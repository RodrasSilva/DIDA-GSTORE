using System.Threading.Tasks;

public interface IStorage
{
    public IPartition GetPartitionOrThrowException(int partitionId);

    public string Read(int partitionId, string objKey);

    public bool IsPartitionMaster(int partitionId);

    public string GetMasterUrl(int partitionId);

    public void Write(int partitionId, string objKey, string objValue, int timestamp);
    public ListServerResponse ListServer();
    public ListGlobalResponse ListGlobal();
}