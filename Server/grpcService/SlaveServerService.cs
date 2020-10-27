using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Server.storage;

namespace DIDA_GSTORE.SlaveServerService
{
    public class SlaveServerService : SlaveService.SlaveServiceBase
    {
        private Storage storage;

        public SlaveServerService(Storage _storage)
        {
            storage = _storage;
        }

        public override Task<LockResponse> lockServer(LockRequest request, ServerCallContext context)
        {
            return base.lockServer(request, context);
        }

        public override Task<UnlockResponse> unlockServer(UnlockRequest request, ServerCallContext context)
        {
            return base.unlockServer(request, context);
        }
    }
}
