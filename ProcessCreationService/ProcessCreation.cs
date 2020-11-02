using System;
using System.Collections.Generic;
using System.Diagnostics;
using DIDA_GSTORE.ServerService;
using Grpc.Core;

namespace ProcessCreationDomain
{
    class ProcessCreation
    {
        private static string _serverFileUrl;
        private static string _clientFileUrl;

        private static ServerService _serverService = new ServerService();
        const int Port = 5001;

        static void Main(string[] args)
        {
            Grpc.Core.Server server = new Grpc.Core.Server
            {
                Services = {ProcessCreationService.BindService(_serverService)},
                Ports = {new ServerPort("localhost", Port, ServerCredentials.Insecure)}
            };
            server.Start();
            Console.WriteLine("ChatServer server listening on port " + Port);
            ReadCommands();

            server.ShutdownAsync().Wait();
        }

        private static void ReadCommands()
        {
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();
        }

        public static void StartServer(string url, int minDelay, int maxDelay, List<Partition> partitions)
        {
            string partitionString = "";
            foreach (Partition p in partitions)
            {
                partitionString += " " + p.id + " " + p.masterUrl;
            }

            Process.Start(_serverFileUrl, url + " " + minDelay + " " + maxDelay + " " + partitionString);
        }

        public static void StartClient(string username, string url, string requestFile, string defaultServerUrl)
        {
            Process.Start(_clientFileUrl, username + " " + url + " " + requestFile + " " + defaultServerUrl);
        }
    }

    public struct Partition
    {
        public int id;
        public string masterUrl;
    }
}