using System;

namespace DIDA_GSTORE.commands {
    public class ListServerCommand : ICommand {
        private const int ServerIdPosition = 0;

        private readonly string _serverId;

        private ListServerCommand(string serverId) {
            _serverId = serverId;
        }

        public static ListServerCommand ParseCommandLine(string[] arguments) {
            if (arguments.Length != 1) {
                throw new Exception("Invalid List Server Command ");
            }

            var serverId = arguments[ServerIdPosition];
            return new ListServerCommand(serverId);
        }

        public void Execute() {
            throw new System.NotImplementedException();
        }
    }
}