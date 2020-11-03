using System;
using PuppetMasterMain;

namespace DIDA_GSTORE.commands {
    public class FreezeRepeatCommand : ICommand {
        private const int ServerIdPosition = 0;

        private readonly int _serverId;

        private FreezeRepeatCommand(int serverId) {
            _serverId = serverId;
        }

        public bool IsAsync => true;
        public bool IsSetup => false;


        public void Execute(PuppetMasterDomain puppetMaster) {
            var response = puppetMaster.GetServerNodeService(_serverId).Freeze();
        }

        public static ICommand ParseCommandLine(string[] arguments) {
            if (arguments.Length != 1) throw new Exception("Invalid Freeze Command ");

            var serverId = arguments[ServerIdPosition];
            return new FreezeRepeatCommand(int.Parse(serverId));
        }
    }
}