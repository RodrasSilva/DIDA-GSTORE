using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace DIDA_GSTORE.ServerService {
    public class NodeService : NodeControlService.NodeControlServiceBase {
        private readonly string _username;

        public NodeService(string username) {
            _username = username;
        }

        public override Task<StatusResponse> status(StatusRequest request, ServerCallContext context) {
            Console.WriteLine($" Status called : {_username}");
            //return base.status(request, context); // TODO : Print status ? 
            return Task.FromResult(new StatusResponse {Status = " ok "});
        }
    }
}