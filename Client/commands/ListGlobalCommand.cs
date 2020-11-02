using System;
using System.Collections.Generic;
using Client.model;
using DIDA_GSTORE.grpcService;

namespace DIDA_GSTORE.commands {
    public class ListGlobalCommand : ICommand {
        private ListGlobalCommand() { }

        public static ListGlobalCommand ParseCommandLine(string[] arguments) {
            return new ListGlobalCommand();
        }

        public void Execute(GrpcService grpcService) {
            List<ListGlobalResult> response = grpcService.ListGlobal();
            foreach (var result in response) {
                result
                    .Identifiers
                    .ForEach(identifier =>
                        Console.WriteLine(
                            $"Partition ${identifier.PartitionId} contains object with id {identifier.ObjectId}"));
            }
        }
    }
}