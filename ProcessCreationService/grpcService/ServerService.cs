using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using ProcessCreationDomain;

namespace DIDA_GSTORE.ServerService{
    public class ServerService : ProcessCreationService.ProcessCreationServiceBase{
        public override Task<StartServerResponse> startServer(StartServerRequest request,
                    ServerCallContext context) {
            var partitions = new List<Partition>();

            foreach (var p in request.Partitions)
            {
                partitions.Add(new Partition { id = p.Id, masterUrl = p.MasterURL });
            }

            ProcessCreation.StartServer(request.ServerId, request.URL, request.MinDelay, request.MaxDelay, partitions);
            
            return Task.FromResult(new StartServerResponse());
        }

        public override Task<StartClientResponse> startClient(StartClientRequest request,
                    ServerCallContext context) {
            ProcessCreation.StartClient(request.Username, request.URL, request.ScriptFile, request.DefaultServerUrl,
                request.Partitions);

            return Task.FromResult(new StartClientResponse());
        }
    }
}