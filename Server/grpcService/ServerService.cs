using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Server.storage;

namespace DIDA_GSTORE.ServerService
{
    public class ServerService : DIDAService.DIDAServiceBase
    {

        Dictionary<string, bool> isMaster = new Dictionary<string, bool>();
        Dictionary<string, List<SlaveService.SlaveServiceClient>> partitionSlaveServers = new Dictionary<string, List<SlaveService.SlaveServiceClient>>();

        private Storage storage;

        public ServerService(Storage _storage)
        {
            storage = _storage;
        }


        public override Task<WriteResponse> write(WriteRequest request, ServerCallContext context)
        {
            return Task.FromResult(Write(request));
        }
        public WriteResponse Write(WriteRequest request)
        {
            return new WriteResponse();
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