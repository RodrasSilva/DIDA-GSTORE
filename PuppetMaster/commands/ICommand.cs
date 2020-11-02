using PuppetMasterMain;

namespace DIDA_GSTORE.commands {
    public interface ICommand {
        public bool IsAsync { get; }
        public bool IsSetup { get; }

        void Execute(PuppetMasterDomain puppetMaster);
    }
}