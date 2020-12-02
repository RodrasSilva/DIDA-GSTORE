using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DIDA_GSTORE.ServerService;
using Grpc.Core;

namespace ProcessCreationDomain {
    internal class ProcessCreation {
        private const int Port = 5001;

        private static readonly ServerService _serverService = new ServerService();

        private static void Main(string[] args) {
            var server = new Server {
                Services = {ProcessCreationService.BindService(_serverService)},
                Ports = {new ServerPort("localhost", Port, ServerCredentials.Insecure)}
            };

            server.Start();
            Console.WriteLine("Process Creation Server listening on port " + Port);
            ReadCommands();

            server.ShutdownAsync().Wait();
        }

        private static void ReadCommands() {
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();
        }

        public static void StartServer(string id, string url,
            float minDelay, float maxDelay, List<Partition> partitions) {
            string partitionString = null;
            foreach (var p in partitions)
                if (partitionString == null) partitionString = p.id;
                else partitionString += " " + p.id;
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine(
                $"Creating server {id} with url {url}, min delay =  {minDelay}, max delay =  {maxDelay}, part of partitions [{partitionString}]");
            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
                @$"..\..\..\..\\Server\\bin\\Debug\\netcoreapp3.1\\Server.exe"));

            var psi = new ProcessStartInfo(path);
            psi.Arguments = $"{id} {url} {minDelay} {maxDelay} {partitionString}";
            psi.UseShellExecute = true;


            Process.Start(psi);
            Console.WriteLine("Finished creating server ");
            Console.WriteLine("----------------------------------------------------\n");
        }

        public static void StartClient(string username, string url, string requestFile, string defaultServerUrl,
            IEnumerable<PartitionClientMessage> partitionInfoMessage) {
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine(
                $"Creating client {username} with url {url}, request file =  {requestFile}, defaultServer =  {defaultServerUrl}");
            var partitionInfo = "";
            foreach (var partition in partitionInfoMessage) {
                partitionInfo += " " + partition.PartitionId + " " + partition.ServerUrls.Count;
                foreach (var serverUrl in partition.ServerUrls) partitionInfo += " " + serverUrl;
            }

            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
                @$"..\..\..\..\\Client\\bin\\Debug\\netcoreapp3.1\\Client.exe"));
            var psi = new ProcessStartInfo(path);
            psi.Arguments = $"{username} {url} {requestFile} {defaultServerUrl} {partitionInfo}";
            psi.UseShellExecute = true;

            Process.Start(psi);
            Console.WriteLine("Finished creating client ");
            Console.WriteLine("----------------------------------------------------\n");
        }
    }

    public struct Partition {
        public string id;
        public string masterUrl;
    }
}