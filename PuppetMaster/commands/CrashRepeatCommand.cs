using System;
using PuppetMasterMain;

namespace DIDA_GSTORE.commands {
    public class CrashRepeatCommand : ICommand {
        private const int ServerIdPosition = 0;

        private readonly int _serverId;

        private CrashRepeatCommand(int serverId) {
            _serverId = serverId;
        }

        public bool IsAsync => true;
        public bool IsSetup => false;


        public void Execute(PuppetMasterDomain puppetMaster) {
            var response = puppetMaster.GetServerNodeService(_serverId).Crash();
        }

        public static ICommand ParseCommandLine(string[] arguments) {
            if (arguments.Length != 1) throw new Exception("Invalid Crash Command ");

            var serverId = arguments[ServerIdPosition];
            return new CrashRepeatCommand(int.Parse(serverId));
        }
    }
}