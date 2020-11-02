using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;

namespace ServerDomain
{
    public class BaseServerStorage : IStorage
    {
        public Dictionary<int, BaseServerPartition> Partitions { get; }

        public IPartition GetPartitionOrThrowException(int partitionId)
        {
            BaseServerPartition partition = null;
            if (Partitions.TryGetValue(partitionId, out partition))
            {
                return partition;
            }

            throw new Exception("No such partition");
        }

        public BaseServerStorage()
        {
            Partitions = new Dictionary<int, BaseServerPartition>();
        }

        public string Read(int partitionId, string objKey)
        {
            return Partitions[partitionId].Read(objKey);
        }

        public bool IsPartitionMaster(int partitionId)
        {
            return Partitions[partitionId].IsMaster;
        }

        public string GetMasterUrl(int partitionId)
        {
            return Partitions[partitionId].GetMasterUrl();
        }
        //Assumes someone called IsMaster (True)
        public void WriteMaster(int partitionId, string objKey, string objValue, int timestamp = -1)
        {
            Partitions[partitionId].WriteMaster(objKey, objValue);
        }

        //Assumes someone called IsMaster (False)
        public void WriteSlave(int partitionId, string objKey, string objValue, int timestamp = -1)
        {
            Partitions[partitionId].WriteSlave(objKey, objValue);
        }

        public ListServerResponse ListServer()
        {
            List<ListServerResponseEntity> objects = new List<ListServerResponseEntity>();

            (new List<int>(Partitions.Keys))
                .ForEach(pId => {
                    var partition = Partitions[pId];
                    var partitionObjects = partition.Objects;
                    (new List<string>(partitionObjects.Keys))
                        .ForEach((objId) => {
                            objects.Add(new ListServerResponseEntity
                            {
                                ObjectValue = partitionObjects[objId].Read(),
                                ObjectId = objId,
                                IsMaster = partition.IsMaster
                            }); ;
                        });
                });
            return new ListServerResponse { Objects = { objects } };
        }

        public ListGlobalResponse ListGlobal()
        {
           // TODO : Implement
        }
    }
}