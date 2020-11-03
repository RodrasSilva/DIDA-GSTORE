using System;
using Client.utils;

namespace Client {
    public static class Client {
        public static void Main(string[] args) {
            if (args.Length != 4) {
                Console.WriteLine("Usage: Client <username> <clientUrl> <operations-file> <defaultServerUrl>");
                return;
            }

            var username = args[0];
            var clientUrl = args[1];
            var operationsFilePath = args[2];
            var defaultServerUrl = args[3];

            var serverParameters = UrlParameters.From(defaultServerUrl);
            var clientParameters = UrlParameters.From(clientUrl);

            var client = new ClientLogic(operationsFilePath,
                username,
                serverParameters.Hostname,
                serverParameters.Port,
                clientParameters.Hostname,
                clientParameters.Port
            );
            client.Execute();
        }
    }
}