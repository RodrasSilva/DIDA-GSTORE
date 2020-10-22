using System;
using System.Collections.Generic;
using Client;
using Client.model;
using DIDA_GSTORE.grpcService;

namespace DIDA_GSTORE.commands {
    public class ListServerCommand : ICommand {
        private const int ServerIdPosition = 0;

        private readonly int _serverId;

        private ListServerCommand(int serverId) {
            _serverId = serverId;
        }

        public static ListServerCommand ParseCommandLine(string[] arguments) {
            if (arguments.Length != 1) {
                throw new Exception("Invalid List Server Command ");
            }

            var serverId = int.Parse(arguments[ServerIdPosition]);
            return new ListServerCommand(serverId);
        }

        public void Execute(GrpcService grpcService) {
            List<ListServerResult> response = grpcService.ListServer(_serverId);
            foreach (var result in response) {
                if (result.IsMaster) {
                    Console.WriteLine($"Server {_serverId} is master with object {result.ObjectValue}");
                }
                else {
                    Console.WriteLine($"Server {_serverId} contains object {result.ObjectValue}");
                }
            }
        }
    }
}