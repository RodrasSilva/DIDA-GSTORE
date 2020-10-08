using DIDA_GSTORE.grpcService;

namespace DIDA_GSTORE.commands
{
    public interface ICommand
    {
        void Execute(GrpcService grpcService);
    }
}