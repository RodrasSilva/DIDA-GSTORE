using System;

namespace Client {
    public static class Client {
        public static void Main(string[] args) {
            if (args.Length != 1) {
                Console.WriteLine("Usage: Client <operations-file>");
                return;
            }

            var operationsFilePath = args[0];
            var client = new ClientLogic(operationsFilePath); 
            client.Execute(); // throws exception if file does not exist
        }
    }
}