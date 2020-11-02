using System;
using System.IO;
using Client.clientNodeServer;
using DIDA_GSTORE.commands;
using DIDA_GSTORE.grpcService;
using Grpc.Core;

namespace Client
{
    public class ClientLogic
    {
        private readonly string _operationsFilePath;
        private readonly GrpcService _grpcService;
        private readonly ClientNodeServer _nodeServer;

        public ClientLogic(string operationsFilePath, string username, string serverHost, int serverPort,
            string clientHost, int clientPort)
        {
            _operationsFilePath = operationsFilePath;
            //TODO : Check what to do with username
            _grpcService = new GrpcService(serverHost, serverPort);
            _nodeServer = new ClientNodeServer(clientHost, clientPort, username, ServerCredentials.Insecure);
        }

        public void Execute()
        {
            ExecuteCommands();
            Console.WriteLine("Operations executed");
            _nodeServer.Start();
            Console.WriteLine("Listening to Status Commands");
            Console.ReadKey();
            _nodeServer.ShutdownAsync();
            Console.WriteLine("Client shutting down...");
        }


        private void ExecuteCommands()
        {
            if (!File.Exists(_operationsFilePath))
            {
                throw new Exception("The given path to the operations file is not valid");
            }

            var commands = ClientCommands.GetCommands(_operationsFilePath);
            commands.ForEach(command => command.Execute(_grpcService));
        }
    }
}