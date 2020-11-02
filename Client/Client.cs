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

            var client = new ClientLogic(operationsFilePath); 
            client.Execute(); // throws exception if file does not exist
        }
    }
}