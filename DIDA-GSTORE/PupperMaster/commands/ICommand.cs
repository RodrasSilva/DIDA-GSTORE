namespace DIDA_GSTORE.commands {
    public interface ICommand {
        public bool IsAsync { get; }

        void Execute();
    }
}