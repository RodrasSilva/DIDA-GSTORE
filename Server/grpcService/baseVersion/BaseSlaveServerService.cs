using System.Threading.Tasks;

public class SlaveServerServiceServer : BaseSlaveService.BaseSlaveServiceBase
{
    private Storage _storage;

    public SlaveServerServiceServer(Storage storage)
    {
        _storage = storage;
    }

   
    public override Task<LockResponse> lockServer(LockRequest request, Grpc.Core.ServerCallContext context)
    {

    }

    public override Task<UnlockResponse> unlockServer(UnlockRequest request, Grpc.Core.ServerCallContext context)
    {

    }
}