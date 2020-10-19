using System;

namespace DIDA_GSTORE.commands {
    public class StatusCommand : ICommand
    {
        public bool IsAsync => true;

        private StatusCommand() {
            
        }
        public void Execute() {
            throw new System.NotImplementedException();
        }

        public static ICommand ParseCommandLine(string[] arguments) {
            //if (arguments.Length != 0) {
            //    throw new Exception("Invalid Status Command ");
            //}
            return new StatusCommand();
        }
    }
}