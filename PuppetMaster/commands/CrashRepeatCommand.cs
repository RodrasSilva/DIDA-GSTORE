using System;
using System.Linq;

namespace DIDA_GSTORE.commands {
    public class CrashRepeatCommand : ICommand {
        public bool IsAsync => true;
        private const int ServerIdPosition = 0;

        private readonly string _serverId;

        private CrashRepeatCommand(string serverId) {
            _serverId = serverId;
        }


        public void Execute() {
            throw new System.NotImplementedException();
        }

        public static ICommand ParseCommandLine(string[] arguments) {
            if (arguments.Length != 1) {
                throw new Exception("Invalid Crash Command ");
            }

            var serverId = arguments[ServerIdPosition];
            return new CrashRepeatCommand(serverId);
        }
    }
}