using System;
using System.IO;
using Client.clientNodeServer;
using DIDA_GSTORE.commands;
using DIDA_GSTORE.grpcService;
using Grpc.Core;

namespace Client {
    public class ClientLogic {
        private readonly string _operationsFilePath;
        private readonly GrpcService _grpcService;

        public ClientLogic(string operationsFilePath) {
            _operationsFilePath = operationsFilePath;

            const string serverHost = "localhost"; // TODO : CHANGE TO DEFAULT SERVER HOSTNAME
            const int serverPort = 50002; // TODO : CHANGE TO DEFAULT SERVER PORT
            _grpcService = new GrpcService(serverHost, serverPort);
        }

        public void Execute() {
            ExecuteCommands();
            Console.WriteLine("Operations executed");
            var nodeServer = new ClientNodeServer("localhost", 50001, ServerCredentials.Insecure);
            nodeServer.Start();
            Console.WriteLine("Listening to Status Commands");
            Console.ReadKey();
            nodeServer.ShutdownAsync();
            Console.WriteLine("Client shutting down...");
        }


        private void ExecuteCommands() {
            if (!File.Exists(_operationsFilePath)) {
                throw new Exception("The given path to the operations file is not valid");
            }

            var commands = ClientCommands.GetCommands(_operationsFilePath);
            commands.ForEach(command => command.Execute(_grpcService));
        }
    }
}