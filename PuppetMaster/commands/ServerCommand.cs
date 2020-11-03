using System;
using PuppetMasterMain;

namespace DIDA_GSTORE.commands {
    public class ServerCommand : ICommand {
        private const int ServerIdPosition = 0;
        private const int UrlPosition = 1;
        private const int MinDelayPosition = 2;
        private const int MaxDelayPosition = 3;
        private readonly int _maxDelay;
        private readonly int _minDelay;

        private readonly int _serverId;
        private readonly string _url;

        private ServerCommand(int serverId, string url, int minDelay, int maxDelay) {
            _serverId = serverId;
            _url = url;
            _minDelay = minDelay;
            _maxDelay = maxDelay;
        }

        public bool IsAsync => true;
        public bool IsSetup => true;


        public void Execute(PuppetMasterDomain puppetMaster) {
            var response = puppetMaster.GetProcessService().StartServer(_serverId,
                _url, _minDelay, _maxDelay);

            //if response is cool
            puppetMaster.AddServer(_serverId, _url);
            //throw new System.NotImplementedException();
        }

        public static ICommand ParseCommandLine(string[] arguments) {
            if (arguments.Length != 4) throw new Exception("Invalid Server Command ");

            var serverId = int.Parse(arguments[ServerIdPosition]);
            var url = arguments[UrlPosition];
            var minDelay = int.Parse(arguments[MinDelayPosition]);
            var maxDelay = int.Parse(arguments[MaxDelayPosition]);
            return new ServerCommand(serverId, url, minDelay, maxDelay);
        }
    }
}