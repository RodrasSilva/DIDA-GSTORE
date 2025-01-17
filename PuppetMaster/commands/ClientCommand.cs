using System;
using PuppetMasterMain;

namespace DIDA_GSTORE.commands {
    public class ClientCommand : ICommand {
        private const int UserNamePosition = 0;
        private const int ClientUrlPosition = 1;
        private const int ScriptFilePosition = 2;
        private readonly string _clientUrl;
        private readonly string _scriptFile;

        private readonly string _username;

        private ClientCommand(string username, string clientUrl, string scriptFile) {
            _username = username;
            _clientUrl = clientUrl;
            _scriptFile = scriptFile;
        }

        public bool IsAsync => true;
        public bool IsSetup => false;


        public void Execute(PuppetMasterDomain puppetMaster) {
            var response = puppetMaster.GetProcessService().StartClient(
                _username, _clientUrl, _scriptFile, puppetMaster.GetDefaultServerUrl(),
                puppetMaster.GetPartitionClientInfo());

            //if response is cool
            puppetMaster.AddClient(_clientUrl);
        }

        public static ICommand ParseCommandLine(string[] arguments) {
            if (arguments.Length != 3) throw new Exception("Invalid Client Command ");

            var username = arguments[UserNamePosition];
            var clientUrl = arguments[ClientUrlPosition];
            var scriptFile = arguments[ScriptFilePosition];
            return new ClientCommand(username, clientUrl, scriptFile);
        }
    }
}