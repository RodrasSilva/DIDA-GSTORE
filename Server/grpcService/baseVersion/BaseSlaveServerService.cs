using System.Threading.Tasks;

namespace Server
{
    public class BaseSlaveServerServiceServer : BaseSlaveService.BaseSlaveServiceBase
    {
        private Storage _storage;

        public BaseSlaveServerServiceServer(Storage storage)
        {
            _storage = storage;
        }


        public override Task<LockResponse> lockServer(LockRequest request, Grpc.Core.ServerCallContext context)
        {
            return base.lockServer(request, context);
        }

        public override Task<UnlockResponse> unlockServer(UnlockRequest request, Grpc.Core.ServerCallContext context)
        {
            return base.unlockServer(request, context);
        }
    }
}