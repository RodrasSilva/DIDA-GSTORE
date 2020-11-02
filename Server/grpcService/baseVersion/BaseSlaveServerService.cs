using System.Threading.Tasks;

namespace ServerDomain
{
    public class BaseSlaveServerService : BaseSlaveService.BaseSlaveServiceBase
    {
        private BaseServerStorage _storage;

        public BaseSlaveServerService(BaseServerStorage storage)
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