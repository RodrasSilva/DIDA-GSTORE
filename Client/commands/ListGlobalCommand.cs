using System;
using DIDA_GSTORE.grpcService;

namespace DIDA_GSTORE.commands{
    public class ListGlobalCommand : ICommand{
        private ListGlobalCommand(){ }

        public void Execute(GrpcService grpcService){
            Console.WriteLine("List Global: \n");
            var response = grpcService.ListGlobal();
           
            string finalResult ="";
            
            foreach (var result in response)
            {
                
                finalResult += $" Partition {result.PartitionId} has: \n";
                foreach(var objResult in result.ObjectIds)
                {
                    finalResult += $" \t {objResult} \n";
                }
                finalResult += "\n";
            }
            Console.Write(finalResult);
        }

        public static ListGlobalCommand ParseCommandLine(string[] arguments){
            return new ListGlobalCommand();
        }
    }
}