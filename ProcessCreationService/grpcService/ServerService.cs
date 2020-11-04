using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using ProcessCreationDomain;

namespace DIDA_GSTORE.ServerService {
    public class ServerService : ProcessCreationService.ProcessCreationServiceBase {
        public override Task<StartServerResponse> startServer(StartServerRequest request,
            ServerCallContext context) {
            var partitions = new List<Partition>();

            foreach (var p in request.Partitions) partitions.Add(new Partition {id = p.Id, masterUrl = p.MasterURL});

            ProcessCreation.StartServer(request.ServerId, request.URL, request.MinDelay, request.MaxDelay, partitions);
            return Task.FromResult(new StartServerResponse());

            //things
            //return base.startServer(request, context);
        }

        public override Task<StartClientResponse> startClient(StartClientRequest request,
            ServerCallContext context) {
            ProcessCreation.StartClient(request.Username, request.URL, request.ScriptFile, request.DefaultServerUrl);
            return Task.FromResult(new StartClientResponse());

            //things
            //return base.startClient(request, context);
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