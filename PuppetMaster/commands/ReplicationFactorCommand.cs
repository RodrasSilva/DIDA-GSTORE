using System;

namespace DIDA_GSTORE.commands {
    public class ReplicationFactorCommand : ICommand {
        public bool IsAsync => true;
        private const int NumberOfServersPosition = 0;
        private readonly int _numberOfServers;

        private ReplicationFactorCommand(int numberOfServers) {
            _numberOfServers = numberOfServers;
        }


        public void Execute() {
            throw new System.NotImplementedException();
        }

        public static ICommand ParseCommandLine(string[] arguments) {
            if (arguments.Length != 1) {
                throw new Exception("Invalid ReplicationFactor Command ");
            }

            var numberOfServers = int.Parse(arguments[NumberOfServersPosition]);
            return new ReplicationFactorCommand(numberOfServers);
        }
    }
}