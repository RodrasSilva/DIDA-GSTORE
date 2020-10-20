using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DIDA_GSTORE.commands;
using DIDA_GSTORE.grpcService;

namespace PuppetMasterMain
{
    public class PuppetMasterDomain
    {
        public Dictionary<int, string> ServerUrls { get; set; }
        public List<string> ClientUrls { get; set; }
        public List<string> PCSUrls { get; set; }

        public GrpcService GrpcService;
        public void Start(string[] args, GrpcService grpcService)
        {
            GrpcService = grpcService;
            /* FIXME according to usage PROBABLY THE SYSTEM CONFIGURATION FILE */
            if (args.Length == 0)
            {
                SetupOperation();
                ExecuteCommands();
                return;

            }
            else if (args.Length == 1)
            {
                SetupOperation();

                var operationsFilePath = args[0];
                if (!File.Exists(operationsFilePath))
                {
                    Console.WriteLine("The given path to the operations file is not valid - App shutting down");
                    return;
                }


                ExecuteCommands(operationsFilePath);
                Console.WriteLine("Operations executed - App shutting down...");
            }
            else
            {
                Console.WriteLine("Usage: PuppetMaster <operations-file>");
                return;
            }
        }
        private void ExecuteCommands()
        {
            List<Thread> allThreads = new List<Thread>();
            string commandLine;
            Console.Write(">>> ");
            while ((commandLine = Console.ReadLine()) != null)
            {
                CommandExecution(commandLine, allThreads);
                Console.Write("\n>>> ");
            }
            foreach (Thread t in allThreads)
            {
                t.Join();
            }
        }

        private void ExecuteCommands(string operationsFilePath)
        {

            var results = new List<ICommand>();
            string commandLine;
            using var operationsFileReader = new StreamReader(operationsFilePath);
            List<Thread> allThreads = new List<Thread>();
            while ((commandLine = operationsFileReader.ReadLine()) != null)
            {
                CommandExecution(commandLine, allThreads);
            }
            foreach (Thread t in allThreads)
            {
                t.Join();
            }
        }

        private void CommandExecution(string commandLine, List<Thread> allThreads)
        {
            try
            {
                ICommand command = PuppetCommands.GetCommand(commandLine);
                if (command.IsAsync)
                {
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
                else
                {
                    command.Execute(this);
                }
            } catch( NotImplementedException e )
            {
                Console.WriteLine(e.Message);
            } catch ( Exception e )
            {
                Console.WriteLine(e.Message);
            }
        }

        private void SetupOperation()
        {
            //starts all relevant processes
            //PuppetMaster will request the PCS to create processes
            //information about the entire set of available PCSs via command line or config file
        }
    }
}
