using Client;
using DIDA_GSTORE.grpcService;

namespace DIDA_GSTORE.commands
{
    public class ListGlobalCommand : ICommand
    {
        private ListGlobalCommand() { }

        public static ListGlobalCommand ParseCommandLine(string[] arguments)
        {
            return new ListGlobalCommand();
        }

        public void Execute(GrpcService grpcService)
        {
            ListGlobalResponse response = grpcService.ListGlobal();
            //TODO :  Logic with response
        }
    }
}