using System;
using PuppetMasterMain;

namespace DIDA_GSTORE.commands {
    public class FreezeRepeatCommand : ICommand {
        private const int ServerIdPosition = 0;

        private readonly string _serverId;
        private readonly bool _discard;

        private FreezeRepeatCommand(string serverId, bool discard) {
            _serverId = serverId;
            _discard = discard;
        }

        public bool IsAsync => false;
        public bool IsSetup => false;


        public void Execute(PuppetMasterDomain puppetMaster) {
            var response = puppetMaster.GetServerNodeService(_serverId).Freeze(_discard);
        }

        public static ICommand ParseCommandLine(string[] arguments) {
            var discard = false;
            if (arguments.Length != 1) {
                if (arguments.Length != 2)
                    throw new Exception("Invalid Freeze Command ");
                else
                    discard = true;
            }

            var serverId = arguments[ServerIdPosition];
            return new FreezeRepeatCommand(serverId, discard);
        }
    }
}