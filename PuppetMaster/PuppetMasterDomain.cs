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
        public List<string> serverIds;
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

        public Dictionary<string, GrpcNodeService> ServerServices{ get; }
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

        public void RemoveServer(string serverId)
        {
            ServerServices.Remove(serverId);
            foreach(var partitionInfo in Partitions)
            {
                partitionInfo.serverIds.Remove(serverId);
            }
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
        }

        public void Start(string[] args,
            GrpcProcessService grpcProcessService){
            PCSs.Add(grpcProcessService);
            
            if (args.Length == 0)
            {
                ExecuteCommands();
            }
            else if (args.Length == 1) 
            {
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
        
        private void ExecuteCommands() {
            var allThreads = new List<Thread>();
            string commandLine;
            Console.Write(">>> ");

            while ((commandLine = Console.ReadLine()) != null) {
                CommandExecution(commandLine, allThreads);
                Console.Write("\n>>> ");
            }

            foreach (var t in allThreads) t.Join();
        }

        private void ExecuteCommands(string operationsFilePath) {
            var results = new List<ICommand>();
            string commandLine;
            using var operationsFileReader = new StreamReader(operationsFilePath);
            var allThreads = new List<Thread>();
            var setupCommands = new List<ICommand>();
            ICommand firstNonSetupCommand = null;

            while ((commandLine = operationsFileReader.ReadLine()) != null)
            {
                if (ParseSetupCommand(commandLine, setupCommands) == false)
                {
                    firstNonSetupCommand = setupCommands[setupCommands.Count - 1];
                    setupCommands.RemoveAt(setupCommands.Count - 1);
                    break;
                }
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

                t.Start();
                allThreads.Add(t);
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

        public List<ServerInfoMessage> GetServerInfoMessages()
        {
            List<ServerInfoMessage> serverInfosDtos = new List<ServerInfoMessage>();
            foreach (var serverId in ServerServices.Keys)
            {
                var serverNode = ServerServices[serverId];
                Console.WriteLine(serverNode.Url);
                serverInfosDtos.Add(new ServerInfoMessage()
                {
                    ServerId = serverId,
                    ServerUrl = "http://" + serverNode.Url
                });
            }
            return serverInfosDtos;
        }

        public List<PartitionInfoMessage> GetPartitionInfo (string serverId)
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
            return partitionInfoDTOs;
        }

        public List<PartitionClientMessage> GetPartitionClientInfo()
        {
            List<PartitionClientMessage> partitionInfoDTOs = new List<PartitionClientMessage>();
            foreach (var partition in Partitions)
            {
                var serverUrls = new List<string>();
                foreach(var serverId in partition.serverIds)
                {
                    serverUrls.Add(ServerServices[serverId].Url);
                }

                Console.WriteLine(partition.masterUrl);
                partitionInfoDTOs.Add(new PartitionClientMessage()
                {
                    PartitionId = partition.partitionId,
                    ServerUrls = { serverUrls },
                });
            }
            return partitionInfoDTOs;

        }

        private void ExecuteSetup(List<ICommand> commands) {
            try {
                var setupThreads = new List<Thread>();
                foreach (var command in commands)
                {
                    command.Execute(this);
                }

                List<ServerInfoMessage> serverInfosDtos = GetServerInfoMessages();

                foreach (var serverId in ServerServices.Keys) 
                {
                    List<PartitionInfoMessage> partitionInfoDTOs = GetPartitionInfo(serverId);

                    var serverPartitions = new List<string>();
                    var serverNode = ServerServices[serverId];

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
    }
}