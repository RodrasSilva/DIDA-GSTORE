using System;
using System.Collections.Generic;
using Grpc.Net.Client;
using Server.utils;

namespace ServerDomain{
    public struct PartitionMasters
    {
        public string partitionId;
        public string masterUrl;
    }
    public class BaseServerStorage : IStorage{

        public Dictionary<string, BaseServerPartition> Partitions { get; }
        public Dictionary<string, string> Servers { get; }


        public BaseServerStorage(){
            Partitions = new Dictionary<string, BaseServerPartition>();
            Servers = new Dictionary<string, string>();
        }

        public List<PartitionMasters> GetPartitionMasters()
        {
            List<PartitionMasters> lists = new List<PartitionMasters>();
            List<PartitionMasters> result = lists;
            foreach(string partitionId in Partitions.Keys)
            {
                result.Add(new PartitionMasters()
                {
                    partitionId = partitionId,
                    masterUrl = Partitions[partitionId].GetMasterUrl()
                });
            }
            return result;
        }

        public void RegisterPartitionSlave(string partitionId, string slaveServerId, string slaveServerUrl){
            //lock (Partitions) {
            var partition = Partitions[partitionId];
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(slaveServerUrl);
            var client = new BaseSlaveService.BaseSlaveServiceClient(channel);
            partition.SlaveServers.Add(new BaseServerPartition.SlaveInfo(slaveServerId, client));
            //}
        }

        public void RegisterPartitionMaster(string partitionId){
            Partitions[partitionId].IsMaster = true;
        }

        public void AddPartition(string partitionId, string masterUrl){
            //lock (Partitions) {
            Console.WriteLine("Creating partition " + partitionId);
            Partitions.Add(partitionId, new BaseServerPartition(partitionId, masterUrl));
            //}
        }

        public IPartition GetPartitionOrThrowException(string partitionId){
            BaseServerPartition partition = null;
            if (Partitions.TryGetValue(partitionId, out partition)) return partition;

            throw new Exception("No such partition");
        }

        public string GetServerOrThrowException(string serverId)
        {
            string s = null;
            if (Servers.TryGetValue(serverId, out s)) return s;

            throw new Exception("No such server");
        }

        public string Read(string partitionId, string objKey){
            return Partitions[partitionId].Read(objKey);
        }

        public string GetMasterUrl(string partitionId){
            return Partitions[partitionId].GetMasterUrl();
        }

        //Assumes someone called IsMaster (True)
        public void WriteMaster(string partitionId, string objKey, string objValue, int timestamp = -1){
            Partitions[partitionId].WriteMaster(objKey, objValue);
        }

        //Assumes someone called IsMaster (False)
        public void WriteSlave(string partitionId, string objKey, string objValue, int timestamp = -1){
            Partitions[partitionId].WriteSlave(objKey, objValue);
        }

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
                            objects.Add(new ListServerResponseEntity {
                                ObjectValue = partitionObjects[objId].Read(),
                                ObjectId = objId,
                                IsMaster = partition.IsMaster
                            });
                            ;
                        });
                });

            Console.WriteLine("ListServer ->");
            objects.ForEach((o) => Console.WriteLine($" Is master = " +
                $"{o.IsMaster}, object {o.ObjectId} with value {o.ObjectValue}"));

            return new ListServerResponse{Objects = {objects}};
        }

        public ListPartitionGlobalResponse ListPartition(string id)
        {
            return new ListPartitionGlobalResponse { ObjectIds = { Partitions[id].Objects.Keys } };
        }

        public bool IsPartitionMaster(string partitionId){
            return Partitions[partitionId].IsMaster;
        }

        public void AddServer(string serverId, string url)
        {
            Servers.Add(serverId, url);
        }
    }
}