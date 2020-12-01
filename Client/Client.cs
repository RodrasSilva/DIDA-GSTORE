using System;
using System.Threading;
using Client.utils;

namespace Client{
    public static class Client {
        private const bool UseBaseVersion = false;

        public static void Main(string[] args){
            if (args.Length < 4){
                Console.WriteLine("Given: " + string.Join(" ", args) +
                                  "Usage: Client <username> <clientUrl> <operations-file> <defaultServerUrl> <partitionId> <number> <serverUrl>");
                return;
            }
            
            var username = args[0];
            Console.Title = "Client: " + username;
            var clientUrl = args[1];
            var operationsFilePath = args[2];
            var defaultServerUrl = args[3];
            string[] partitions = new string[args.Length - 4];
            for(int i = 4; i < args.Length; i++)
            {
                partitions[i - 4] = args[i];
            }

            foreach (var item in args)
            {
                Console.WriteLine(item.ToString());
            }
            foreach (var item in partitions)
            {
                Console.WriteLine(item.ToString());
            }

            var serverParameters = UrlParameters.From(defaultServerUrl);
            var clientParameters = UrlParameters.From(clientUrl);

            var client = new ClientLogic(operationsFilePath,
                username,
                serverParameters.Hostname,
                serverParameters.Port,
                clientParameters.Hostname,
                clientParameters.Port,
                partitions,
                UseBaseVersion
            );
            client.Execute();
        }
    }
}