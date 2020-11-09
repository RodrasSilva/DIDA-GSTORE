using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Server.utils;

namespace DIDA_GSTORE.ServerService{
    public class NodeService : NodeControlService.NodeControlServiceBase{
        private FreezeUtilities freezeUtilities;

        public NodeService(FreezeUtilities freezeUtilities)
        {
            this.freezeUtilities = freezeUtilities;
        }

        public override Task<StatusResponse> status(StatusRequest request, ServerCallContext context){
            ServerDomain.Server.DelayMessage();
            Console.WriteLine("Status called");
            return Task.FromResult(new StatusResponse{Status = " ok "});
            //return base.status(request, context);
        }

        public override Task<CrashResponse> crash(CrashRequest request, ServerCallContext context){
            ServerDomain.Server.DelayMessage();

            //return base.crash(request, context);
            var t = new Thread(() => CrashMechanism());
            t.Start();
            //new CrashResponse();
            return Task.FromResult(new CrashResponse());
        }

        public void CrashMechanism(){
            ServerDomain.Server.DelayMessage();

            //Thread.Sleep(1000);
            Environment.Exit(1);
        }

        public override Task<FreezeResponse> freeze(FreezeRequest request, ServerCallContext context){
            
            Console.WriteLine("Freezing server");
            freezeUtilities.Freeze();
            return Task.FromResult(new FreezeResponse());
        }

        public override Task<UnfreezeResponse> unfreeze(UnfreezeRequest request, ServerCallContext context){
            
            Console.WriteLine("Unfreezing server");
            freezeUtilities.Unfreeze();
            return Task.FromResult(new UnfreezeResponse());
        }

        // partitionsInfo format : <partitionId1> <partitionMasterServerURLN1> ... <partitionIdN> <partitionMasterServerURLN> |
        // <partitionIdM> ... 
        //or
        // partitionsInfo format: <partitionId1> <partitionMasterServerURLN1> <isHere>... <partitionIdN> <partitionMasterServerURLN> <isHere>
         public override Task<CompleteSetupResponse> completeSetup(CompleteSetupRequest request,
            ServerCallContext context){
            Console.WriteLine("Server - here");


            var partitionsInfo = request.Partitions.ToArray();
            var serversInfo = request.ServerInfo.ToArray();


            ServerDomain.Server.RegisterServers(serversInfo);
            ServerDomain.Server.RegisterPartitions(partitionsInfo);
            return Task.FromResult(new CompleteSetupResponse());
        }


        /*
         public void SetupComplete() {
            ServerDomain.Server.RegisterPartitions();
         }

         */
    }
}