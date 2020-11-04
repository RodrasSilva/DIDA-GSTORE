using System;
using PuppetMasterMain;

namespace DIDA_GSTORE.commands{
    public class ReplicationFactorCommand : ICommand{
        private const int NumberOfServersPosition = 0;
        private readonly int _numberOfServers;

        private ReplicationFactorCommand(int numberOfServers){
            _numberOfServers = numberOfServers;
        }

        public bool IsAsync => true;
        public bool IsSetup => true;


        public void Execute(PuppetMasterDomain puppetMaster){
            puppetMaster.ReplicationFactor = _numberOfServers;
        }

        public static ICommand ParseCommandLine(string[] arguments){
            if (arguments.Length != 1) throw new Exception("Invalid ReplicationFactor Command ");

            var numberOfServers = int.Parse(arguments[NumberOfServersPosition]);
            return new ReplicationFactorCommand(numberOfServers);
        }
    }
}