using System;
using PuppetMasterClient;
using PuppetMasterMain;

namespace DIDA_GSTORE.commands {
    public class UnfreezeRepeatCommand : ICommand {
        public bool IsAsync => true;
        public bool IsSetup => false;

        private const int ServerIdPosition = 0;
        private readonly int _serverId;

        private UnfreezeRepeatCommand(int serverId) {
            _serverId = serverId;
        }


        public void Execute(PuppetMasterDomain puppetMaster) {
            UnfreezeResponse response = puppetMaster.GetServerNodeService(_serverId).Unfreeze();
        }

        public static ICommand ParseCommandLine(string[] arguments) {
            if (arguments.Length != 1) {
                throw new Exception("Invalid Unfreeze Command ");
            }

            var serverId = arguments[ServerIdPosition];
            return new UnfreezeRepeatCommand(int.Parse(serverId));
        }
    }
}