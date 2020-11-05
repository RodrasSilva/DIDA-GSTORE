using System;
using PuppetMasterMain;

namespace DIDA_GSTORE.commands{
    public class ServerCommand : ICommand{
        private const int ServerIdPosition = 0;
        private const int UrlPosition = 1;
        private const int MinDelayPosition = 2;
        private const int MaxDelayPosition = 3;
        private readonly float _maxDelay;
        private readonly float _minDelay;

        private readonly string _serverId;
        private readonly string _url;

        private ServerCommand(string serverId, string url,
            float minDelay, float maxDelay){
            _serverId = serverId;
            _url = url;
            _minDelay = minDelay;
            _maxDelay = maxDelay;
        }

        public bool IsAsync => true;
        public bool IsSetup => true;


        public void Execute(PuppetMasterDomain puppetMaster){
            var response = puppetMaster.GetProcessService().StartServer(
                _serverId, _url, _minDelay, _maxDelay);

            //if response is cool
            puppetMaster.AddServer(_serverId, _url);
            //throw new System.NotImplementedException();
        }

        public static ICommand ParseCommandLine(string[] arguments){
            if (arguments.Length != 4) throw new Exception("Invalid Server Command ");

            var serverId = arguments[ServerIdPosition];
            var url = arguments[UrlPosition];
            var minDelay = float.Parse(arguments[MinDelayPosition]);
            var maxDelay = float.Parse(arguments[MaxDelayPosition]);
            return new ServerCommand(serverId, url, minDelay, maxDelay);
        }
    }
}