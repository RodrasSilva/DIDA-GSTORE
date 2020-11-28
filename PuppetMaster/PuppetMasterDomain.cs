using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DIDA_GSTORE.commands;
using DIDA_GSTORE.grpcService;
using Google.Protobuf.Collections;
using PuppetMasterClient;

namespace PuppetMasterMain{
    public struct PartitionInfo {
        public string partitionId;
        public string masterUrl;
        public string[] serverIds;
    }


    public struct ServerInfoDTO
    {
        public string serverId;
        public string serverUrl;
    }

    public struct PartitionInfoDTO
    {
        public string partitionId;
        public string partitionMasterUrl;
        public bool isMyPartition;
    }
    public class PuppetMasterDomain{
        public PuppetMasterDomain(){
            ServerServices = new Dictionary<string, GrpcNodeService>();
            ServersUrls = new Dictionary<string, string>();
            ClientServices = new List<GrpcNodeService>();
            PCSs = new List<GrpcProcessService>();
            Partitions = new List<PartitionInfo>();
        }

        private List<GrpcProcessService> PCSs{ get; }

        private Dictionary<string, GrpcNodeService> ServerServices{ get; }
        private Dictionary<string, string> ServersUrls{ get; }
        public List<GrpcNodeService> ClientServices{ get; set; }
        public int ReplicationFactor{ get; set; }
        //public Dictionary<string, List<PartitionInfo>> partitionsPerServer{ get; set; }
        public List<PartitionInfo> Partitions { get; set; }

        public List<GrpcNodeService> GetAllNodeServices(){
            var result = new List<GrpcNodeService>();
            result.AddRange(ClientServices);
            result.AddRange(ServerServices.Values);
            return result;
        }

        public void AddServer(string serverId, string url){
            ServerServices.Add(serverId, PuppetMaster.urlToNodeService(url));
            ServersUrls.Add(serverId, url);
        }

        public void AddClient(string url){
            ClientServices.Add(PuppetMaster.urlToNodeService(url));
        }

        public GrpcProcessService GetProcessService(){
            if (PCSs.Count == 0) throw new Exception("No PCS");
            return PCSs[0];
        }

        public GrpcNodeService GetServerNodeService(string serverId){
            var grpc = ServerServices[serverId];
            if (grpc == null) throw new Exception("No such server");
            return grpc;
        }

        public string GetDefaultServerUrl(){
            foreach (var grpcNodeService in ServerServices.Values) return grpcNodeService.Url;
            throw new Exception("No servers");
            //return ServerServices[ServerServices.Keys[new Random().Next(0, ServerServices.Keys.Count)].Url;
        }

        public void Start(string[] args,
            GrpcProcessService grpcProcessService){
            PCSs.Add(grpcProcessService);

            //test
            //var grpcNodeService = new GrpcNodeService("localhost", 5001);
            //ServerServices.Add("1", grpcNodeService);
            //GrpcNodeService = grpcNodeService;

            /* FIXME according to usage PROBABLY THE SYSTEM CONFIGURATION FILE */
            if (args.Length == 0){
                SetupOperation();
                ExecuteCommands();
            }
            else if (args.Length == 1){
                SetupOperation();
                var  operationsFilePath = Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, @$"..\..\..\scripts\{args[0]}"));
                if (!File.Exists(operationsFilePath)){
                    Console.WriteLine("The given path: " + operationsFilePath +
                                      ". to the operations file is not valid - App shutting down");
                    return;
                }


                ExecuteCommands(operationsFilePath);
                Console.WriteLine("Operations executed - App shutting down...");
            }
            else{
                Console.WriteLine("Usage: PuppetMaster <operations-file>");
            }
        }

        private void ExecuteCommands(){
            var allThreads = new List<Thread>();
            string commandLine;
            Console.Write(">>> ");
            while ((commandLine = Console.ReadLine()) != null){
                CommandExecution(commandLine, allThreads);
                Console.Write("\n>>> ");
            }

            foreach (var t in allThreads) t.Join();
        }

        private void ExecuteCommands(string operationsFilePath){
            var results = new List<ICommand>();
            string commandLine;
            using var operationsFileReader = new StreamReader(operationsFilePath);
            var allThreads = new List<Thread>();
            var setupCommands = new List<ICommand>();
            ICommand firstNonSetupCommand = null;

            while ((commandLine = operationsFileReader.ReadLine()) != null)
                if (ParseSetupCommand(commandLine, setupCommands) == false){
                    firstNonSetupCommand = setupCommands[setupCommands.Count - 1];
                    setupCommands.RemoveAt(setupCommands.Count - 1);
                    break;
                }

            ExecuteSetup(setupCommands);

            ExecuteGivenCommand(firstNonSetupCommand, allThreads);

            while ((commandLine = operationsFileReader.ReadLine()) != null) CommandExecution(commandLine, allThreads);

            foreach (var t in allThreads) t.Join();
        }

        private void ExecuteGivenCommand(ICommand command, List<Thread> allThreads){
            if (command.IsAsync){
                var puppetMaster = this;
                var t = new Thread(() => command.Execute(puppetMaster));

                // Start ThreadProc.  Note that on a uniprocessor, the new
                // thread does not get any processor time until the main thread
                // is preempted or yields.  Uncomment the Thread.Sleep that
                // follows t.Start() to see the difference.
                t.Start();
                allThreads.Add(t);
                //we might have to sleep
                //Thread.Sleep(0);
            }
            else{
                command.Execute(this);
                Console.WriteLine("teste3");
            }
        }

        private void CommandExecution(string commandLine, List<Thread> allThreads){
            try{
                var command = PuppetCommands.GetCommand(commandLine);
                ExecuteGivenCommand(command, allThreads);
            }
            catch (NotImplementedException e){
                Console.WriteLine(e.Message);
            }
            catch (Exception e){
                Console.WriteLine(e.Message);
            }
        }

        private bool ParseSetupCommand(string commandLine, List<ICommand> setupCommands){
            try{
                var command = PuppetCommands.GetCommand(commandLine);
                setupCommands.Add(command);
                return command.IsSetup;
            }
            catch (Exception e){
                Console.WriteLine(e.Message);
            }

            return false;
        }

        private void ExecuteSetup(List<ICommand> commands){
            try{
                var setupThreads = new List<Thread>();
                foreach (var command in commands)
                    //all setup commands will be linear
                    command.Execute(this);
                //foreach (var t in setupThreads) t.Join();
                List<ServerInfoMessage> serverInfosDtos = new List<ServerInfoMessage>();
                foreach (var serverId in ServerServices.Keys)
                {
                    var serverNode = ServerServices[serverId];
                    Console.WriteLine(serverNode.Url);
                    serverInfosDtos.Add(new ServerInfoMessage() {
                        ServerId = serverId,
                        ServerUrl = "http://" + serverNode.Url
                    });
                }

                foreach (var serverId in ServerServices.Keys) 
                {
                    List<PartitionInfoMessage> partitionInfoDTOs = new List<PartitionInfoMessage>();
                    foreach (var partition in Partitions)
                    {
                        Console.WriteLine(partition.masterUrl);
                        partitionInfoDTOs.Add(new PartitionInfoMessage()
                        {
                            PartitionId = partition.partitionId,
                            PartitionMasterUrl = ServersUrls[partition.masterUrl],
                            IsMyPartition = partition.serverIds.Contains(serverId), //change later
                            ServerIds = { partition.serverIds },
                        });
                    }

                    // Send partition info to server as <partitionId1> <partitionMasterServerURLN1> ... <partitionIdN> <partitionMasterServerURLN>

                    var serverPartitions = new List<string>();
                    var serverNode = ServerServices[serverId];
                    
                    /*
                    var serverPartitionsInfo = partitionsPerServer[serverId];

                    Console.WriteLine("Puppet Master - Completing setup for server " + serverId);
                    serverPartitionsInfo.ForEach(p => {
                        Console.WriteLine("Puppet Master - Completing setup for server " + serverId +
                                          " - Adding partition " + p.partitionId + " with master " +
                                          ServersUrls[p.masterUrl]);
                        serverPartitions.Add(p.partitionId);
                        serverPartitions.Add(ServersUrls[p.masterUrl]);
                    });
                    */
                    serverNode.CompleteSetup(serverInfosDtos, partitionInfoDTOs);
                }
            }
            catch (NotImplementedException e){
                Console.WriteLine(e.Message);
                Console.WriteLine("here");
            }
            catch (Exception e){
                Console.WriteLine(e.Message);
                Console.WriteLine("here 2.0");
            }

            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine("Finished servers setup");
            Console.WriteLine("----------------------------------------------------");
        }

        private void SetupOperation(){
            //starts all relevant processes
            //PuppetMaster will request the PCS to create processes
            //information about the entire set of available PCSs via command line or config file
        }
    }
}