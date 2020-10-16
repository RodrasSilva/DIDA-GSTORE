using System;
using Client;
using DIDA_GSTORE.grpcService;

namespace DIDA_GSTORE.commands
{
    public class ListServerCommand : ICommand
    {
        private const int ServerIdPosition = 0;

        private readonly string _serverId;

        private ListServerCommand(string serverId)
        {
            _serverId = serverId;
        }

        public static ListServerCommand ParseCommandLine(string[] arguments)
        {
            if (arguments.Length != 1)
            {
                throw new Exception("Invalid List Server Command ");
            }

            var serverId = arguments[ServerIdPosition];
            return new ListServerCommand(serverId);
        }

        public void Execute(GrpcService grpcService)
        {
            ListServerResponse response = grpcService.ListServer(_serverId);
            //TODO :  Logic with response
        }
    }
}