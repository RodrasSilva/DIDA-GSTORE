using System;
using System.Threading.Tasks;
using Grpc.Core;
using Server;
using System.Threading;

namespace DIDA_GSTORE.ServerService {
    public class NodeService : NodeControlService.NodeControlServiceBase {
        public override Task<StatusResponse> status(StatusRequest request, ServerCallContext context) {
            return base.status(request, context);
        }

        public override Task<CrashResponse> crash(CrashRequest request, ServerCallContext context) {
            //return base.crash(request, context);
            Thread t = new Thread(() => CrashMechanism());
            t.Start();
            //new CrashResponse();
            return Task.FromResult(new CrashResponse());
        }

        public void CrashMechanism() {
            Thread.Sleep(1000);
            Environment.Exit(1);
        }

        public override Task<FreezeResponse> freeze(FreezeRequest request, ServerCallContext context) {
            return base.freeze(request, context);
        }

        public override Task<UnfreezeResponse> unfreeze(UnfreezeRequest request, ServerCallContext context) {
            return base.unfreeze(request, context);
        }
    }
}