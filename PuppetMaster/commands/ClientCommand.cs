using PuppetMasterClient;
using PuppetMasterMain;
using System;
using System.Linq;

namespace DIDA_GSTORE.commands {
    public class ClientCommand : ICommand {
        public bool IsAsync => true;
        private const int UserNamePosition = 0;
        private const int ClientUrlPosition = 1;
        private const int ScriptFilePosition = 2;

        private readonly string _username;
        private readonly string _clientUrl;
        private readonly string _scriptFile;

        private ClientCommand(string username, string clientUrl, string scriptFile) {
            _username = username;
            _clientUrl = clientUrl;
            _scriptFile = scriptFile;
        }


        public void Execute(PuppetMasterDomain puppetMaster) {
            //throw new System.NotImplementedException();

            StartClientResponse response = puppetMaster.GrpcService.StartClient(_username,
                _clientUrl, _scriptFile);

            //if response is cool
            puppetMaster.ClientUrls.Add(_clientUrl);
        }

        public static ICommand ParseCommandLine(string[] arguments) {
            if (arguments.Length != 3) {
                throw new Exception("Invalid Client Command ");
            }

            var username = arguments[UserNamePosition];
            var clientUrl = arguments[ClientUrlPosition];
            var scriptFile = arguments[ScriptFilePosition];
            return new ClientCommand(username, clientUrl, scriptFile);
        }
    }
}