using System;
using DIDA_GSTORE.grpcService;

namespace DIDA_GSTORE.commands{
    public class ListGlobalCommand : ICommand{
        private ListGlobalCommand(){ }

        public void Execute(GrpcService grpcService){
            var response = grpcService.ListGlobal();
            Console.WriteLine(response.Count);

            string finalResult ="";
            Console.Write("teste1");
            foreach (var result in response)
            {
                Console.Write("teste2");

                finalResult += $" Partition {result.PartitionId} has: \n";
                foreach(var objResult in result.ObjectIds)
                {
                    finalResult += $" \t {objResult} \n";
                }
                finalResult += "\n";
            }
            Console.Write("teste3");
            Console.Write(finalResult);
        }

        public static ListGlobalCommand ParseCommandLine(string[] arguments){
            return new ListGlobalCommand();
        }
    }
}