using System;
using System.Linq;

namespace DIDA_GSTORE.commands {
    public abstract class PuppetCommands {
        private const string ArgumentSeparator = " ";
        private const string ReplicationFactorCommandName = "ReplicationFactor";
        private const string ServerCommandName = "Server";
        private const string PartitionCommandName = "Partition";
        private const string ClientCommandName = "Client";
        private const string StatusCommandName = "Status";
        private const string CrashRepeatCommandName = "Crash";
        private const string FreezeRepeatCommandName = "Freeze";
        private const string UnfreezeRepeatCommandName = "Unfreeze";
        private const string WaitCommandName = "Wait";

        public static ICommand GetCommand(string commandLine) {
            var splitLine = commandLine.Split(ArgumentSeparator);
            var commandName = splitLine[0];
            var args = splitLine.Skip(1).ToArray();
            return ParseCommand(commandName, args);
        }

        private static ICommand ParseCommand(string commandName, string[] args) {
            return commandName switch {
                ReplicationFactorCommandName => ReplicationFactorCommand.ParseCommandLine(args),
                ServerCommandName => ServerCommand.ParseCommandLine(args),
                PartitionCommandName => PartitionCommand.ParseCommandLine(args),
                ClientCommandName => ClientCommand.ParseCommandLine(args),
                StatusCommandName => StatusCommand.ParseCommandLine(args),
                CrashRepeatCommandName => CrashRepeatCommand.ParseCommandLine(args),
                FreezeRepeatCommandName => FreezeRepeatCommand.ParseCommandLine(args),
                UnfreezeRepeatCommandName => UnfreezeRepeatCommand.ParseCommandLine(args),
                WaitCommandName => WaitCommand.ParseCommandLine(args),
                _ => throw new Exception("Command not found")
            };
        }
    }
}