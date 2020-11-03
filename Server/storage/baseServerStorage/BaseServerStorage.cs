using System;
using System.Collections.Generic;
using Grpc.Net.Client;

namespace ServerDomain {
    public class BaseServerStorage : IStorage {
        public BaseServerStorage() {
            Partitions = new Dictionary<string, BaseServerPartition>();
        }

        public Dictionary<string, BaseServerPartition> Partitions { get; }

        public void RegisterPartitionSlave(string partitionId, string slaveServerId, string slaveServerUrl) {
            //lock (Partitions) {
            var partition = Partitions[partitionId];
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(slaveServerUrl);
            var client = new BaseSlaveService.BaseSlaveServiceClient(channel);
            partition.SlaveServers.Add(new BaseServerPartition.SlaveInfo(slaveServerId, client));
            //}
        }

        public IPartition GetPartitionOrThrowException(string partitionId) {
            BaseServerPartition partition = null;
            if (Partitions.TryGetValue(partitionId, out partition)) return partition;

            throw new Exception("No such partition");
        }

        public string Read(string partitionId, string objKey) {
            return Partitions[partitionId].Read(objKey);
        }

        public string GetMasterUrl(string partitionId) {
            return Partitions[partitionId].GetMasterUrl();
        }

        //Assumes someone called IsMaster (True)
        public void WriteMaster(string partitionId, string objKey, string objValue, int timestamp = -1) {
            Partitions[partitionId].WriteMaster(objKey, objValue);
        }

        //Assumes someone called IsMaster (False)
        public void WriteSlave(string partitionId, string objKey, string objValue, int timestamp = -1) {
            Partitions[partitionId].WriteSlave(objKey, objValue);
        }

        public ListServerResponse ListServer() {
            var objects = new List<ListServerResponseEntity>();

            new List<string>(Partitions.Keys)
                .ForEach(pId => {
                    var partition = Partitions[pId];
                    var partitionObjects = partition.Objects;
                    new List<string>(partitionObjects.Keys)
                        .ForEach(objId => {
                            objects.Add(new ListServerResponseEntity {
                                ObjectValue = partitionObjects[objId].Read(),
                                ObjectId = objId,
                                IsMaster = partition.IsMaster
                            });
                            ;
                        });
                });
            return new ListServerResponse {Objects = {objects}};
        }

        public ListGlobalResponse ListGlobal() {
            // TODO : Implement
            return new ListGlobalResponse();
        }


        public bool IsPartitionMaster(string partitionId) {
            return Partitions[partitionId].IsMaster;
        }
    }
}