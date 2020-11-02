using System;
using PuppetMasterClient;
using PuppetMasterMain;

namespace DIDA_GSTORE.commands
{
    public class CrashRepeatCommand : ICommand
    {
        public bool IsAsync => true;
        public bool IsSetup => false;

        private const int ServerIdPosition = 0;

        private readonly int _serverId;

        private CrashRepeatCommand(int serverId)
        {
            _serverId = serverId;
        }


        public void Execute(PuppetMasterDomain puppetMaster)
        {
            CrashResponse response = puppetMaster.GetServerNodeService(_serverId).Crash();
        }

        public static ICommand ParseCommandLine(string[] arguments)
        {
            if (arguments.Length != 1)
            {
                throw new Exception("Invalid Crash Command ");
            }

            var serverId = arguments[ServerIdPosition];
            return new CrashRepeatCommand(int.Parse(serverId));
        }
    }
}