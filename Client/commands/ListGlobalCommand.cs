using System;
using DIDA_GSTORE.grpcService;

namespace DIDA_GSTORE.commands {
    public class ListGlobalCommand : ICommand {
        private ListGlobalCommand() { }

        public void Execute(GrpcService grpcService) {
            Console.WriteLine("List Global: \n");
            var response = grpcService.ListGlobal();
            foreach (var wrapperRes in response) {
                Console.WriteLine("List server for server: " + wrapperRes.Key);
                foreach (var result in wrapperRes.Value)
                    if (result.IsMaster)
                        Console.WriteLine(
                            $"Server {wrapperRes.Key} is master with object [{result.ObjectId},{result.ObjectValue}]");
                    else
                        Console.WriteLine(
                            $"Server {wrapperRes.Key} contains object [{result.ObjectId},{result.ObjectValue}]");
            }

            /*
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
            */
        }

        public static ListGlobalCommand ParseCommandLine(string[] arguments) {
            return new ListGlobalCommand();
        }
    }
}