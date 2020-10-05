using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace DIDA_GSTORE.commands {
    public abstract class ClientCommands {

        private const string ArgumentSeparator = " ";
        private const string ReplicationFactorCommandName = "ReplicationFactor";
        private const string ServerCommandName = "Server";
        private const string PartitionCommandName = "Partition";
        private const string ClientGlobalCommandName = "Client";
        private const string StatusCommandName = "Status";
        private const string CrashRepeatCommandName = "Crash";
        private const string FreezeRepeatCommandName = "Freeze";
        private const string UnfreezeRepeatCommandName = "Unfreeze";
        private const string WaitRepeatCommandName = "Wait";

        public static ICommand GetCommand(string commandLine) {
            var splitLine = commandLine.Split(ArgumentSeparator);
            var commandName = splitLine[0];
            var args = splitLine.Skip(1).ToArray();
            return ParseCommand(commandName, args);
        }

        private static ICommand ParseCommand(string commandName, string[] args) {
            return commandName switch {
                ReplicationFactorCommandName => ReadCommand.ParseCommandLine(args),
                ServerCommandName => WriteCommand.ParseCommandLine(args),
                PartitionCommandName => ListServerCommand.ParseCommandLine(args),
                ClientGlobalCommandName => ListGlobalCommand.ParseCommandLine(args),
                StatusCommandName => WaitCommand.ParseCommandLine(args),
                CrashRepeatCommandName => WaitCommand.ParseCommandLine(args),
                FreezeRepeatCommandName => WaitCommand.ParseCommandLine(args),
                UnfreezeRepeatCommandName => WaitCommand.ParseCommandLine(args),
                WaitRepeatCommandName => WaitCommand.ParseCommandLine(args),
                _ => null
            };
        }
    }
}