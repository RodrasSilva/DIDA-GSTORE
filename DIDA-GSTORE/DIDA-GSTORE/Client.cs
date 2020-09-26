using System;
using System.IO;
using DIDA_GSTORE.commands;
using Grpc.Core;

namespace DIDA_GSTORE {
    public class Client {
        public static void Main(string[] args) {
            if (args.Length != 1) {
                Console.WriteLine("Usage: Client <operations-file>");
                return;
            }

            var operationsFilePath = args[0];
            if (!File.Exists(operationsFilePath)) {
                Console.WriteLine("The given path to the operations file is not valid - App shutting down");
                return;
            }

            ExecuteCommands(operationsFilePath);
            Console.WriteLine("Operations executed - App shutting down...");
        }

        private static void ExecuteCommands(string operationsFilePath) {
            using var operationsFileReader = new StreamReader(operationsFilePath);
            string commandLine;
            while ((commandLine = operationsFileReader.ReadLine()) != null) {
                ICommand command = ClientCommands.GetCommand(commandLine, operationsFileReader);
                command.Execute();
            }
        }
    }
}