using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DIDA_GSTORE.commands;

namespace PuppetMaster
{
    class PuppetMaster
    {
        public static void Main(string[] args)
        {
            /* FIXME according to usage PROBABLY THE SYSTEM CONFIGURATION FILE */
            if (args.Length == 0)
            {
                SetupOperation();


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
        private static void ExecuteCommands()
        {
            List<Thread> allThreads = new List<Thread>();
            string commandLine;
            while ((commandLine = Console.ReadLine()) != null)
            {
                CommandExecution(commandLine, allThreads);
            }
            foreach (Thread t in allThreads)
            {
                t.Join();
            }
        }

        private static void ExecuteCommands(string operationsFilePath)
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

        private static void CommandExecution(string commandLine, List<Thread> allThreads)
        {
            ICommand command = PuppetCommands.GetCommand(commandLine);
            if (command.IsAsync)
            {
                Thread t = new Thread(new ThreadStart(command.Execute));

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
                command.Execute();
            }
        }

        private static void SetupOperation()
        {
            //starts all relevant processes
            //PuppetMaster will request the PCS to create processes
            //information about the entire set of available PCSs via command line or config file
        }
    }
}
