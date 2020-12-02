using System;
using System.Collections.Generic;
using System.IO;
using Client.clientNodeServer;
using DIDA_GSTORE.commands;
using DIDA_GSTORE.grpcService;
using Grpc.Core;

namespace Client {
    public class ClientLogic {
        private readonly GrpcService _grpcService;
        private readonly ClientNodeServer _nodeServer;
        private readonly string _operationsFilePath;

        public Dictionary<string, List<string>> ServerList = new Dictionary<string, List<string>>();

        public ClientLogic(string operationsFilePath, string username, string serverHost, int serverPort,
            string clientHost, int clientPort, string[] partitions, bool useAdvanced) {
            _operationsFilePath = operationsFilePath;

            _grpcService = new GrpcService(serverHost, serverPort, this, useAdvanced);
            _nodeServer = new ClientNodeServer(clientHost, clientPort, username, ServerCredentials.Insecure);

            ParsePartitions(new List<string>(partitions));
        }

        public void ParsePartitions(List<string> partitions) {
            Console.WriteLine(partitions);
            try {
                while (partitions.Count > 0) {
                    //partitions[0];
                    var partitionId = partitions[0];
                    var serverCount = int.Parse(partitions[1]);

                    partitions.RemoveAt(0);
                    partitions.RemoveAt(0);

                    var serverUrls = new List<string>();
                    for (var i = 0; i < serverCount; i++) {
                        serverUrls.Add(partitions[0]);
                        partitions.RemoveAt(0);
                    }

                    ServerList.Add(partitionId, serverUrls);
                }
            }
            catch (ArgumentOutOfRangeException e) {
                Console.WriteLine(e.Message);
                Console.WriteLine("eUrl parameters are not correct");
            }
        }

        public void Execute() {
            ExecuteCommands();
            Console.WriteLine("Operations executed");
            _nodeServer.Start();

            Console.WriteLine("Listening to Status Commands");
            Console.ReadKey();

            _nodeServer.ShutdownAsync();
            Console.WriteLine("Client shutting down...");
        }


        private void ExecuteCommands() {
            var operationsFilePath =
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @$"..\..\..\scripts\{_operationsFilePath}"));
            Console.WriteLine("Client reading file : " + operationsFilePath + " \n ");
            if (!File.Exists(operationsFilePath))
                throw new Exception("The given path to the operations file is not valid, file: " + operationsFilePath);

            var commands = ClientCommands.GetCommands(operationsFilePath);
            commands.ForEach(command => { command.Execute(_grpcService); });
        }

        public List<string> GetServerUrlList() {
            var serverUrls = new List<string>();

            foreach (var server in ServerList)
            foreach (var serverUrl in server.Value)
                if (!serverUrls.Contains(serverUrl))
                    serverUrls.Add(serverUrl);

            return serverUrls;
        }
    }
}