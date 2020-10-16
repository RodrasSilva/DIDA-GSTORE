using System;
using System.Threading.Tasks;
using Grpc.Core;
using Server;

namespace DIDA_GSTORE.ServerService
{
    public class ServerService : DIDAService.DIDAServiceBase
    {
        public override Task<WriteResponse> write(WriteRequest request, ServerCallContext context)
        {
            return base.write(request, context);
        }

        public override Task<ReadResponse> read(ReadRequest request, ServerCallContext context)
        {
            return base.read(request, context);
        }

        public override Task<ListGlobalResponse> listGlobal(ListGlobalRequest request, ServerCallContext context)
        {
            return base.listGlobal(request, context);
        }

        public override Task<ListServerResponse> listServer(ListServerRequest request, ServerCallContext context)
        {
            return base.listServer(request, context);
        }
    }
}