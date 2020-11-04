using System;
using System.Threading;
using PuppetMasterMain;

namespace DIDA_GSTORE.commands{
    public class WaitCommand : ICommand{
        private const int WaitTimePosition = 0;
        private readonly int _waitTime;

        private WaitCommand(int waitTime){
            _waitTime = waitTime;
        }

        public bool IsAsync => true;
        public bool IsSetup => false;

        public void Execute(PuppetMasterDomain puppetMaster){
            Thread.Sleep(_waitTime);
        }

        public static WaitCommand ParseCommandLine(string[] arguments){
            if (arguments.Length != 1) throw new Exception("Invalid Wait Command ");

            var waitTime = int.Parse(arguments[WaitTimePosition]);
            return new WaitCommand(waitTime);
        }
    }
}