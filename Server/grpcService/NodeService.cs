using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace DIDA_GSTORE.ServerService
{
    public class NodeService : NodeControlService.NodeControlServiceBase
    {
        public override Task<StatusResponse> status(StatusRequest request, ServerCallContext context)
        {
            ServerDomain.Server.DelayMessage();
            return base.status(request, context);
        }

        public override Task<CrashResponse> crash(CrashRequest request, ServerCallContext context)
        {
            ServerDomain.Server.DelayMessage();

            //return base.crash(request, context);
            Thread t = new Thread(() => CrashMechanism());
            t.Start();
            //new CrashResponse();
            return Task.FromResult(new CrashResponse());
        }

        public void CrashMechanism()
        {
            ServerDomain.Server.DelayMessage();

            Thread.Sleep(1000);
            Environment.Exit(1);
        }

        public override Task<FreezeResponse> freeze(FreezeRequest request, ServerCallContext context)
        {
            ServerDomain.Server.DelayMessage();

            return base.freeze(request, context);
        }

        public override Task<UnfreezeResponse> unfreeze(UnfreezeRequest request, ServerCallContext context)
        {
            ServerDomain.Server.DelayMessage();
            return base.unfreeze(request, context);
        }
    }
}