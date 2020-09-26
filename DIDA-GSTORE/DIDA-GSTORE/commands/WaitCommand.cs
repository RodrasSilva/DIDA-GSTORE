using System;

namespace DIDA_GSTORE.commands {
    public class WaitCommand : ICommand {
        private const int WaitTimePosition = 0;

        private readonly long _waitTime;

        private WaitCommand(long waitTime) {
            _waitTime = waitTime;
        }

        public static WaitCommand ParseCommandLine(string[] arguments) {
            if (arguments.Length != 1) {
                throw new Exception("Invalid Wait Command ");
            }

            var waitTime = long.Parse(arguments[WaitTimePosition]);
            return new WaitCommand(waitTime);
        }

        public void Execute() {
            throw new System.NotImplementedException();
        }
    }
}