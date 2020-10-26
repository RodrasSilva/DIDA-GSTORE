using System;
using System.Threading.Tasks;
using Grpc.Core;
using System.Threading;
using Client;

namespace DIDA_GSTORE.ServerService
{
    public class NodeService : NodeControlService.NodeControlServiceBase
    {
        public override Task<StatusResponse> status(StatusRequest request, ServerCallContext context)
        {
            return base.status(request, context);
        }

        public override Task<CrashResponse> crash(CrashRequest request, ServerCallContext context)
        {
            return base.crash(request, context);
        }

        public void CrashMechanism()
        {
           //ignore
        }

        public override Task<FreezeResponse> freeze(FreezeRequest request, ServerCallContext context)
        {
            return base.freeze(request, context);
        }

        public override Task<UnfreezeResponse> unfreeze(UnfreezeRequest request, ServerCallContext context)
        {
            return base.unfreeze(request, context);
        }
    }
}