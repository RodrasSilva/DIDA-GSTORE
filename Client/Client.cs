using System;
using Client.utils;

namespace Client
{
    public static class Client
    {
        public static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: Client <username> <clientUrl> <operations-file> <defaultServerUrl>");
                return;
            }

            string username = args[0];
            string clientUrl = args[1];
            string operationsFilePath = args[2];
            string defaultServerUrl = args[3];

            UrlParameters serverParameters = UrlParameters.From(defaultServerUrl);
            UrlParameters clientParameters = UrlParameters.From(clientUrl);

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