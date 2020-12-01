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
        private Action delayFunction;
        private Action<ServerInfo[]> registerServersFunction;
        private Action<PartitionInfo[]> registerPartitionsFunction;

        public NodeService(FreezeUtilities freezeUtilities, Action delayFunction, Action<ServerInfo[]>  registerServersFunction,Action<PartitionInfo[]> registerPartitionsFunction)
        {
            this.freezeUtilities = freezeUtilities;
            this.delayFunction = delayFunction;
            this.registerServersFunction = registerServersFunction;
            this.registerPartitionsFunction = registerPartitionsFunction;
        }

        public override Task<StatusResponse> status(StatusRequest request, ServerCallContext context){
            delayFunction();
            Console.WriteLine("Status called");
            return Task.FromResult(new StatusResponse{Status = " ok "});
        }

        public override Task<CrashResponse> crash(CrashRequest request, ServerCallContext context){
            delayFunction();

            var t = new Thread(() => CrashMechanism());
            t.Start();

            return Task.FromResult(new CrashResponse());
        }

        public void CrashMechanism(){
            delayFunction();

            Thread.Sleep(1000);
            Environment.Exit(1);
        }

        public override Task<FreezeResponse> freeze(FreezeRequest request, ServerCallContext context){
            
            Console.WriteLine("Freezing server");
            if (request.Discard)
            {
                freezeUtilities.Discard();
            }
            else
            {
                freezeUtilities.Freeze();
            }
            return Task.FromResult(new FreezeResponse());
        }

        public override Task<UnfreezeResponse> unfreeze(UnfreezeRequest request, ServerCallContext context){
            
            Console.WriteLine("Unfreezing server");
            freezeUtilities.Unfreeze();
            return Task.FromResult(new UnfreezeResponse());
        }

        public override Task<CompleteSetupResponse> completeSetup(CompleteSetupRequest request,
                    ServerCallContext context){
            Console.WriteLine("Server - here");


            var partitionsInfo = request.Partitions.ToArray();
            var serversInfo = request.ServerInfo.ToArray();


            registerServersFunction(serversInfo);
            registerPartitionsFunction(partitionsInfo);
            return Task.FromResult(new CompleteSetupResponse());
        }


      
    }
}