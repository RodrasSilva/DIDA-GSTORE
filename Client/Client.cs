using System;
using System.IO;
using Client.clientNodeServer;
using DIDA_GSTORE.commands;
using DIDA_GSTORE.grpcService;
using DIDA_GSTORE.ServerService;
using Grpc.Core;

namespace Client {
    public class Client {
        private static GrpcService grpcService;
        private static NodeService _nodeService = new NodeService();
        const int Port = 5001;

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

            grpcService = new GrpcService("localhost", 5001);
            ExecuteCommands(operationsFilePath);
            Console.WriteLine("Operations executed");

            var nodeServer = new ClientNodeServer("localhost", Port, ServerCredentials.Insecure, _nodeService);
            nodeServer.Start();
            Console.ReadKey();
            nodeServer.ShutdownAsync();
            Console.WriteLine("App shutting down...");
        }

        private static void ExecuteCommands(string operationsFilePath) {
            var commands = ClientCommands.GetCommands(operationsFilePath);
            commands.ForEach(command => command.Execute(grpcService));
        }
    }
}