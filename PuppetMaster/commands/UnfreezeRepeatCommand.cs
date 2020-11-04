using System;
using PuppetMasterMain;

namespace DIDA_GSTORE.commands {
    public class UnfreezeRepeatCommand : ICommand {
        private const int ServerIdPosition = 0;
        private readonly string _serverId;

        private UnfreezeRepeatCommand(string serverId) {
            _serverId = serverId;
        }

        public bool IsAsync => true;
        public bool IsSetup => false;


        public void Execute(PuppetMasterDomain puppetMaster) {
            var response = puppetMaster.GetServerNodeService(_serverId).Unfreeze();
        }

        public static ICommand ParseCommandLine(string[] arguments) {
            if (arguments.Length != 1) throw new Exception("Invalid Unfreeze Command ");

            var serverId = arguments[ServerIdPosition];
            return new UnfreezeRepeatCommand(serverId);
        }
    }
}