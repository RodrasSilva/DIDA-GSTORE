namespace DIDA_GSTORE.commands {
    public class ListGlobalCommand : ICommand {
        private ListGlobalCommand() { }

        public static ListGlobalCommand ParseCommandLine(string[] arguments) {
            return new ListGlobalCommand();
        }

        public void Execute() {
            throw new System.NotImplementedException();
        }
    }
}