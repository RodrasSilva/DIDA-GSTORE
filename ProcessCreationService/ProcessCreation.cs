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
        //private static string _serverFileUrl ="D:\\RandomnessD\\MEIC_4ANO_1SEMESTRE\\DAD\\DIDA-GSTORE\\Server\\Server.csproj";

        //private static string _clientFileUrl = "../Client/Client.cs";

        private static readonly ServerService _serverService = new ServerService();

        private static void Main(string[] args) {
            var server = new Server {
                Services = {ProcessCreationService.BindService(_serverService)},
                Ports = {new ServerPort("localhost", Port, ServerCredentials.Insecure)}
            };
            //StartServer("localhost:8000", 0f, 0f, new List<Partition>());
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
            foreach (var p in partitions){
                if(partitionString == null) partitionString = p.id;
                else partitionString += " " + p.id;
            }
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine($"Creating server {id} with url {url}, min delay =  {minDelay}, max delay =  {maxDelay}, part of partitions [{partitionString}]");
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\Server\\bin\\Debug\\netcoreapp3.1\\Server.exe");

            path = "C:\\Users\\Rodrigo Silva\\Desktop\\DAD\\Project\\DIDA-GSTORE\\Server\\bin\\Debug\\netcoreapp3.1\\Server.exe";
            //path = "..\\Server\\bin\\Debug\\netcoreapp3.1\\Server.exe";
            //Console.WriteLine($"Starting server with arguments :{id} {url} {minDelay} {maxDelay} {partitionString} ");
            Process.Start(path, $"{id} {url} {minDelay} {maxDelay} {partitionString}");
            Console.WriteLine("Finished creating server ");
            Console.WriteLine("----------------------------------------------------\n");
        }

        public static void StartClient(string username, string url, string requestFile, string defaultServerUrl) {
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine($"Creating client {username} with url {url}, request file =  {requestFile}, defaultServer =  {defaultServerUrl}");
            
            var path = "C:\\Users\\Rodrigo Silva\\Desktop\\DAD\\Project\\DIDA-GSTORE\\Client\\bin\\Debug\\netcoreapp3.1\\Client.exe"; ;

            Process.Start(path, username + " " + url + " " + requestFile + " " + defaultServerUrl);
            Console.WriteLine("Finished creating client ");
            Console.WriteLine("----------------------------------------------------\n");
        }
    }

    public struct Partition {
        public string id;
        public string masterUrl;
    }
}