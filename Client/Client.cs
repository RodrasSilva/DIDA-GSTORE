using System;

namespace Client {
    public static class Client {
        public static void Main(string[] args) {
            /*
            if (args.Length != 1) {
                Console.WriteLine("Usage: Client <operations-file>");
                return;
            }*/
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: Client <username> <clientUrl> <operations-file>");
                return;
            }

            string username = args[0];
            string clientUrl = args[1];
            string operationsFilePath = args[2];
            string defaultServerUrl = args[3];

            string[] parsedUrl = defaultServerUrl.Split(':');
            if (parsedUrl.Length != 2)
            {
                throw new Exception("Bad format on url: " + parsedUrl);
            }
            string serverHost = parsedUrl[0];
            int serverPort = int.Parse(parsedUrl[1]);

            parsedUrl = clientUrl.Split(':');
            if (parsedUrl.Length != 2)
            {
                throw new Exception("Bad format on url: " + parsedUrl);
            }
            string clientHost = parsedUrl[0];
            int clientPort = int.Parse(parsedUrl[1]);

            var client = new ClientLogic(operationsFilePath, serverHost, serverPort); 
            client.Execute(clientHost, clientPort); // throws exception if file does not exist
        }
    }
}