using PuppetMasterMain;
using System;

namespace DIDA_GSTORE.commands {
    public class FreezeRepeatCommand : ICommand {
        public bool IsAsync => true;
        private const int ServerIdPosition = 0;

        private readonly string _serverId;

        private FreezeRepeatCommand(string serverId) {
            _serverId = serverId;
        }


        public void Execute(PuppetMasterDomain puppetMaster) {
            throw new System.NotImplementedException();
        }

        public static ICommand ParseCommandLine(string[] arguments) {
            if (arguments.Length != 1) {
                throw new Exception("Invalid Freeze Command ");
            }

            var serverId = arguments[ServerIdPosition];
            return new FreezeRepeatCommand(serverId);
        }
    }
}