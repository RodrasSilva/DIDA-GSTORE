using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DIDA_GSTORE.commands{
    public abstract class ClientCommands{
        private const string ArgumentSeparator = " ";
        private const string ReadCommandName = "read";
        private const string WriteCommandName = "write";
        private const string ListServerCommandName = "listServer";
        private const string ListGlobalCommandName = "listGlobal";
        private const string WaitCommandName = "wait";
        private const string BeginRepeatCommandName = "begin-repeat";

        public static List<ICommand> GetCommands(string operationsFilePath){
            var results = new List<ICommand>();
            string commandLine;
            using var operationsFileReader = new StreamReader(operationsFilePath);
            while ((commandLine = operationsFileReader.ReadLine()) != null)
                results.Add(GetCommand(commandLine, operationsFileReader));

            return results;
        }

        public static ICommand GetCommand(string commandLine, StreamReader operationsFileReader = null){
            var splitLine = commandLine.Split(ArgumentSeparator);
            Console.WriteLine("Client executing " + commandLine);
            var commandName = splitLine[0];
            var args = splitLine.Skip(1).ToArray();
            return ParseCommand(commandName, args, operationsFileReader);
        }

        private static ICommand ParseCommand(string commandName, string[] args, StreamReader operationsFileReader){
            return commandName switch{
                ReadCommandName => ReadCommand.ParseCommandLine(args),
                WriteCommandName => WriteCommand.ParseCommandLine(args),
                ListServerCommandName => ListServerCommand.ParseCommandLine(args),
                ListGlobalCommandName => ListGlobalCommand.ParseCommandLine(args),
                WaitCommandName => WaitCommand.ParseCommandLine(args),
                BeginRepeatCommandName => BeginRepeatCommand.ParseCommandLine(args, operationsFileReader),
                _ => null
            };
        }
    }
}