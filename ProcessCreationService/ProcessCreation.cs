using System;
using System.Collections.Generic;
using System.Diagnostics;
using DIDA_GSTORE.ServerService;
using Grpc.Core;

namespace ProcessCreationDomain {
    internal class ProcessCreation {
        private const int Port = 5001;
        private static string _serverFileUrl;
        private static string _clientFileUrl;

        private static readonly ServerService _serverService = new ServerService();

        private static void Main(string[] args) {
            var server = new Server {
                Services = {ProcessCreationService.BindService(_serverService)},
                Ports = {new ServerPort("localhost", Port, ServerCredentials.Insecure)}
            };
            server.Start();
            Console.WriteLine("ChatServer server listening on port " + Port);
            ReadCommands();

            server.ShutdownAsync().Wait();
        }

        private static void ReadCommands() {
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();
        }

        public static void StartServer(string url, int minDelay, int maxDelay, List<Partition> partitions) {
            var partitionString = "";
            foreach (var p in partitions) partitionString += " " + p.id + " " + p.masterUrl;

            Process.Start(_serverFileUrl, url + " " + minDelay + " " + maxDelay + " " + partitionString);
        }

        public static void StartClient(string username, string url, string requestFile, string defaultServerUrl) {
            Process.Start(_clientFileUrl, username + " " + url + " " + requestFile + " " + defaultServerUrl);
        }
    }

    public struct Partition {
        public int id;
        public string masterUrl;
    }
}