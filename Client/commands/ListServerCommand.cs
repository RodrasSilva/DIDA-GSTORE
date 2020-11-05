using System;
using DIDA_GSTORE.grpcService;

namespace DIDA_GSTORE.commands{
    public class ListServerCommand : ICommand{
        private const int ServerIdPosition = 0;

        private readonly string _serverId;

        private ListServerCommand(string serverId){
            _serverId = serverId;
        }

        public void Execute(GrpcService grpcService){
            Console.WriteLine($"List Server [{_serverId}]: \n");
            var response = grpcService.ListServer(_serverId);
            foreach (var result in response)
                if (result.IsMaster)
                    Console.WriteLine(
                        $"Server {_serverId} is master with object [{result.ObjectId},{result.ObjectValue}]");
                else
                    Console.WriteLine($"Server {_serverId} contains object [{result.ObjectId},{result.ObjectValue}]");
        }

        public static ListServerCommand ParseCommandLine(string[] arguments){
            if (arguments.Length != 1) throw new Exception("Invalid List Server Command ");

            var serverId = arguments[ServerIdPosition];
            return new ListServerCommand(serverId);
        }
    }
}