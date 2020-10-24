using PuppetMasterClient;
using PuppetMasterMain;
using System;

namespace DIDA_GSTORE.commands {
    public class FreezeRepeatCommand : ICommand {
        public bool IsAsync => true;
        private const int ServerIdPosition = 0;

        private readonly int _serverId;

        private FreezeRepeatCommand(int serverId) {
            _serverId = serverId;
        }


        public void Execute(PuppetMasterDomain puppetMaster) {
            FreezeResponse response = puppetMaster.GetServerNodeService(_serverId).Freeze();
        }

        public static ICommand ParseCommandLine(string[] arguments) {
            if (arguments.Length != 1) {
                throw new Exception("Invalid Freeze Command ");
            }

            var serverId = arguments[ServerIdPosition];
            return new FreezeRepeatCommand(int.Parse(serverId));
        }
    }
}