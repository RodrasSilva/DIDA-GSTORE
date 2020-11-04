using PuppetMasterMain;

namespace DIDA_GSTORE.commands{
    public class StatusCommand : ICommand{
        private StatusCommand(){ }

        public bool IsAsync => true;
        public bool IsSetup => false;

        public void Execute(PuppetMasterDomain puppetMaster){
            foreach (var grpc in puppetMaster.GetAllNodeServices()) grpc.Status();
        }

        public static ICommand ParseCommandLine(string[] arguments){
            //if (arguments.Length != 0) {
            //    throw new Exception("Invalid Status Command ");
            //}
            return new StatusCommand();
        }
    }
}