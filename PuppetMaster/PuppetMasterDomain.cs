using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using DIDA_GSTORE.commands;
using DIDA_GSTORE.grpcService;

namespace PuppetMasterMain {
    public class PuppetMasterDomain {
        private Dictionary<int, GrpcNodeService> ServerServices { get; set; }
        public List<GrpcNodeService> ClientServices { get; set; }
        private List<GrpcProcessService> PCSs;

        public PuppetMasterDomain() {
            ServerServices = new Dictionary<int, GrpcNodeService>();
            ClientServices = new List<GrpcNodeService>();
            PCSs = new List<GrpcProcessService>();
        }

        public void AddServer(int serverId, string url) {
            ServerServices.Add(serverId, PuppetMaster.urlToNodeService(url));
        }

        public void AddClient(string url) {
            ClientServices.Add(PuppetMaster.urlToNodeService(url));
        }

        public GrpcProcessService GetProcessService() {
            if (PCSs.Count == 0) throw new Exception("No PCS");
            return PCSs[0];
        }

        public GrpcNodeService GetServerNodeService(int serverId) {
            GrpcNodeService grpc = ServerServices[serverId];
            if (grpc == null) throw new Exception("No such server");
            return grpc;
        }

        public void Start(string[] args,
            GrpcProcessService grpcProcessService) {
            PCSs.Add(grpcProcessService);

            GrpcNodeService grpcNodeService = new GrpcNodeService("localhost", 5001);
            ServerServices.Add(1, grpcNodeService);
            //GrpcNodeService = grpcNodeService;

            /* FIXME according to usage PROBABLY THE SYSTEM CONFIGURATION FILE */
            if (args.Length == 0) {
                SetupOperation();
                ExecuteCommands();
                return;
            }
            else if (args.Length == 1) {
                SetupOperation();

                var operationsFilePath = args[0];
                if (!File.Exists(operationsFilePath)) {
                    Console.WriteLine("The given path to the operations file is not valid - App shutting down");
                    return;
                }


                ExecuteCommands(operationsFilePath);
                Console.WriteLine("Operations executed - App shutting down...");
            }
            else {
                Console.WriteLine("Usage: PuppetMaster <operations-file>");
                return;
            }
        }

        private void ExecuteCommands() {
            List<Thread> allThreads = new List<Thread>();
            string commandLine;
            Console.Write(">>> ");
            while ((commandLine = Console.ReadLine()) != null) {
                CommandExecution(commandLine, allThreads);
                Console.Write("\n>>> ");
            }

            foreach (Thread t in allThreads) {
                t.Join();
            }
        }

        private void ExecuteCommands(string operationsFilePath) {
            var results = new List<ICommand>();
            string commandLine;
            using var operationsFileReader = new StreamReader(operationsFilePath);
            List<Thread> allThreads = new List<Thread>();
            while ((commandLine = operationsFileReader.ReadLine()) != null) {
                CommandExecution(commandLine, allThreads);
            }

            foreach (Thread t in allThreads) {
                t.Join();
            }
        }

        private void CommandExecution(string commandLine, List<Thread> allThreads) {
            try {
                ICommand command = PuppetCommands.GetCommand(commandLine);
                if (command.IsAsync) {
                    PuppetMasterDomain puppetMaster = this;
                    Thread t = new Thread(() => command.Execute(puppetMaster));

                    // Start ThreadProc.  Note that on a uniprocessor, the new
                    // thread does not get any processor time until the main thread
                    // is preempted or yields.  Uncomment the Thread.Sleep that
                    // follows t.Start() to see the difference.
                    t.Start();
                    allThreads.Add(t);
                    //we might have to sleep
                    //Thread.Sleep(0);
                }
                else {
                    command.Execute(this);
                }
            }
            catch (NotImplementedException e) {
                Console.WriteLine(e.Message);
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        private void SetupOperation() {
            //starts all relevant processes
            //PuppetMaster will request the PCS to create processes
            //information about the entire set of available PCSs via command line or config file
        }
    }
}