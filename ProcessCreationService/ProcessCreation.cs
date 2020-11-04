using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DIDA_GSTORE.ServerService;
using Grpc.Core;

namespace ProcessCreationDomain {
    internal class ProcessCreation {
        private const int Port = 5001;
        //private static string _serverFileUrl = "../Server/Server.cs";
        private static string _serverFileUrl =
        "D:\\RandomnessD\\MEIC_4ANO_1SEMESTRE\\DAD\\DIDA-GSTORE\\Server\\Server.csproj";

        private static string _clientFileUrl = "../Client/Client.cs";

        private static readonly ServerService _serverService = new ServerService();

        private static void Main(string[] args) {
            var server = new Server {
                Services = {ProcessCreationService.BindService(_serverService)},
                Ports = {new ServerPort("localhost", Port, ServerCredentials.Insecure)}
            };
            //StartServer("localhost:8000", 0f, 0f, new List<Partition>());
            server.Start();
            Console.WriteLine("ChatServer server listening on port " + Port);
            ReadCommands();

            server.ShutdownAsync().Wait();
        }

        private static void ReadCommands() {
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();
        }

        public static void StartServer(string id, string url, 
            float minDelay, float maxDelay, List<Partition> partitions) {
            var partitionString = "";
            foreach (var p in partitions) partitionString += " " + p.id + " " + p.masterUrl;
            
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\Server\\bin\\Debug\\netcoreapp3.1\\Server.exe");

            path = "D:\\RandomnessD\\MEIC_4ANO_1SEMESTRE\\DAD\\DIDA-GSTORE\\Server\\bin\\Debug\\netcoreapp3.1\\Server.exe";
            //path = "..\\Server\\bin\\Debug\\netcoreapp3.1\\Server.exe";
            Process.Start(path, id + " " + url + " " + minDelay + " " + maxDelay + " " + partitionString);
        }

        public static void StartClient(string username, string url, string requestFile, string defaultServerUrl) {
            var path = "D:\\RandomnessD\\MEIC_4ANO_1SEMESTRE\\DAD\\DIDA-GSTORE\\Client\\bin\\Debug\\netcoreapp3.1\\Client.exe"; ;

            Process.Start(path, username + " " + url + " " + requestFile + " " + defaultServerUrl);
        }
    }

    public struct Partition {
        public string id;
        public string masterUrl;
    }
}