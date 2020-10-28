using System;
using System.Threading.Tasks;
using Grpc.Core;
using System.Threading;
using Client;

namespace DIDA_GSTORE.ServerService
{
    public class NodeService : NodeControlService.NodeControlServiceBase
    {
        public override Task<StatusResponse> status(StatusRequest request, ServerCallContext context) {
            return base.status(request, context); // TODO : Print status ? 
        }
    }
}