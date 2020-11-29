using System;
using System.Collections.Generic;
using Grpc.Net.Client;
using Server.utils;
using static AdvancedServerObjectInfo;

namespace ServerDomain{


    public class AdvancedServerStorage : IStorage {

        public class ServerExtraInfo
        {
            public ServerExtraInfo(string serverId, string serverUrl, AdvancedSlaveService.AdvancedSlaveServiceClient serverChannel)
            {
                ServerId = serverId;
                ServerUrl = serverUrl;
                ServerChannel = serverChannel;
            }
            public string ServerId { get; }
            public string ServerUrl { get; }
            public AdvancedSlaveService.AdvancedSlaveServiceClient ServerChannel { get; }
        }

        public string ServerId { get; set; }
        public string ServerUrl { get; set; }

        public Dictionary<string, AdvancedServerPartition> Partitions { get; }
        public List<ServerExtraInfo> Servers { get; }

        public AdvancedServerStorage(){
            Partitions = new Dictionary<string, AdvancedServerPartition>();
            Servers = new List<ServerExtraInfo>();
        }

        //stuff from base server 
        public List<PartitionMasters> GetPartitionMasters()
        {
            List<PartitionMasters> lists = new List<PartitionMasters>();
            List<PartitionMasters> result = lists;
            foreach (string partitionId in Partitions.Keys)
            {
                result.Add(new PartitionMasters()
                {
                    partitionId = partitionId,
                    masterUrl = Partitions[partitionId].GetMasterUrl()
                });
            }
            return result;
        }
        /* Mudar depois quando se for implementar o RAFT Election Process

        public void RegisterPartitionSlave(string partitionId, string slaveServerId, string slaveServerUrl)
        {

            //lock (Partitions) {
            var partition = Partitions[partitionId];
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(slaveServerUrl);
            var client = new BaseSlaveService.BaseSlaveServiceClient(channel);
            partition.SlaveServers.Add(new BaseServerPartition.SlaveInfo(slaveServerId, client));
            //}
        }
        */
        public void RegisterPartitionSlave(string partitionId, string slaveServerId, string slaveServerUrl)
        {
            throw new NotImplementedException();
        }

        //use this and register all servers to each other
        public void RegisterServer(string partitionId, string serverId, string serverUrl)
        {
            var partition = Partitions[partitionId];
            foreach(var server in Servers)
            {
                if(server.ServerId.Equals(serverId))
                {
                    partition.Servers.Add(new AdvancedServerPartition.ServerInfo(serverId, server.ServerChannel));
                    break;
                }
            }
        }

        public List<ServerExtraInfo> GetServersNotFromPartition(string partitionId)
        {
            var partition = Partitions[partitionId];
            List<ServerExtraInfo> result = new List<ServerExtraInfo>();
            foreach (var server in Servers)
            {
                bool inPar = false;
                foreach(var parServer in partition.Servers)
                {
                    if(server.ServerId.Equals(parServer.ServerId))
                    {
                        inPar = true;
                        break;
                    }
                }
                if(!inPar)
                {
                    result.Add(server);
                }
            }
            return result;
        }

        public void RegisterPartitionMaster(string partitionId)
        {
            Partitions[partitionId].IsMaster = true;
        }

        public void AddPartition(string partitionId, string masterUrl)
        {
            //lock (Partitions) {
            Console.WriteLine("Creating partition " + partitionId);
            Partitions.Add(partitionId, new AdvancedServerPartition(partitionId, masterUrl, this));
            //}
        }

        public string GetServerOrThrowException(string serverId)
        {
            foreach(var server in Servers)
            {
                if(server.ServerId.Equals(serverId))
                {
                    return server.ServerUrl;
                }
            }
            //if (Servers.TryGetValue(serverId, out s)) return s;

            throw new Exception("No such server");
        }


        public ObjectVal ReadAdvanced(string partitionId, string objKey,
            string clientObjectValue, int clientTimestamp)
        {
            return Partitions[partitionId].ReadAdvanced(objKey,
                clientObjectValue, clientTimestamp);
        }

        public string Read(string partitionId, string objKey)
        {
            throw new NotImplementedException();
        }

        public string GetMasterUrl(string partitionId){
            return Partitions[partitionId].GetMasterUrl();
        }

        public IPartition GetPartitionOrThrowException(string partitionId){
            AdvancedServerPartition partition = null;
            if (Partitions.TryGetValue(partitionId, out partition)) return partition;

            throw new Exception("No such partition");
        }

        //end of stuff from base server 

        public ListServerResponse ListServer(){
            var objects = new List<ListServerResponseEntity>();

            new List<string>(Partitions.Keys)
                .ForEach(pId => {
                    var partition = Partitions[pId];
                    var partitionObjects = partition.Objects;
                    //Console.WriteLine("About to add objects");
                    //Console.WriteLine(partitionObjects.Keys.Count);
                    //Console.WriteLine(new List<string>(partitionObjects.Keys));
                    new List<string>(partitionObjects.Keys)
                        .ForEach(objId => {
                            //Console.WriteLine("Adding a object");
                            objects.Add(new ListServerResponseEntity
                            {
                                ObjectValue = partitionObjects[objId].Read().value,
                                ObjectId = objId,
                                IsMaster = partition.IsMaster
                            });
                            ;
                        });
                });

            Console.WriteLine("ListServer ->");
            objects.ForEach((o) => Console.WriteLine($" Is master = " +
                $"{o.IsMaster}, object {o.ObjectId} with value {o.ObjectValue}"));

            return new ListServerResponse { Objects = { objects } };
        }

        public void WriteMaster(string partitionId, string objKey, string objValue, int timestamp){
            Partitions[partitionId].WriteMaster(objKey, objValue);
        }

        public void WriteSlave(string partitionId, string objKey, string objValue, int timestamp){
            Partitions[partitionId].WriteSlave(objKey, objValue, timestamp);
        }

        public bool IsPartitionMaster(string partitionId){
            return Partitions[partitionId].IsMaster;
        }

        public void Write(string partitionId, string objKey, string objValue, int timestamp = -1){
            Partitions[partitionId].Write(objKey, objValue, timestamp);
        }

        public void AddServer(string serverId, string url)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(url);
            var client = new AdvancedSlaveService.AdvancedSlaveServiceClient(channel);
            Servers.Add(new ServerExtraInfo(serverId, url, client));
        }

        public ListPartitionGlobalResponse ListPartition(string id)
        {
            return new ListPartitionGlobalResponse { ObjectIds = { Partitions[id].Objects.Keys } };
        }

        public void ResetTimeout(string partitionId)
        {
            Partitions[partitionId].ResetTimeout();
        }

        public void SetSlaveTimeout(string partitionId)
        {
            Partitions[partitionId].SetSlaveTimeout();
        }
        /*
        public bool IsMasterInAnyPartition()
        {
            foreach(var partition in Partitions.Values)
            {
                if(partition.IsMaster)
                {
                    return true;
                }
            }
            return false;
        }
        */
    }
}