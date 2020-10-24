using System;
using System.Threading.Tasks;
using Grpc.Core;
using ProcessCreation;

namespace DIDA_GSTORE.ServerService
{
    public class ServerService : ProcessCreationService.ProcessCreationServiceBase
    {
        public override Task<StartServerResponse> startServer(StartServerRequest request,
            ServerCallContext context)
        {
            //things
            return base.startServer(request, context);
        }
        public override Task<StartClientResponse> startClient(StartClientRequest request,
            ServerCallContext context)
        {
            //things
            return base.startClient(request, context);
        }
        /*
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
        */
    }
}