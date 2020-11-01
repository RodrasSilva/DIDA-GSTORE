using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using static SlaveService;

namespace Server.storage {
    public static class StorageUtils {
        public static Partition GetPartitionOrThrowException(this Storage input, int partitionId) {
            Partition partition = null;
            if (input.Partitions.TryGetValue(partitionId, out partition)) {
                return partition;
            }

            throw new Exception("No such partition");
        }
    }


    public class Storage {
        public Dictionary<int, Partition> Partitions { get; }

        public Storage() {
            Partitions = new Dictionary<int, Partition>();
        }


        public string Read(int partitionId, string objKey) {
            return Partitions[partitionId].Read(objKey);
        }

        public bool IsPartitionMaster(int partitionId) {
            return Partitions[partitionId].IsMaster;
        }

        public string GetMasterUrl(int partitionId) {
            return Partitions[partitionId].GetMasterUrl();
        }

        public void Write(int partitionId, string objKey, string objValue, int timestamp = -1) {
            Partitions[partitionId].Write(objKey, objValue, timestamp);
        }
    }

    public class Partition {
        public bool IsMaster { get; set; }
        private int _partitionId;
        private List<SlaveServiceClient> slaveServiceClients;
        private string _masterUrl;

        public Dictionary<string, ObjectInfo> Objects { get; }

        public Partition(int partitionId, string masterUrl) {
            _partitionId = partitionId;
            _masterUrl = masterUrl;
            Objects = new Dictionary<string, ObjectInfo>();
            slaveServiceClients = new List<SlaveServiceClient>();
        }

        public string GetMasterUrl() {
            return _masterUrl; //temporary solution
        }

        public string Read(string objKey) {
            return Objects[objKey].Read();
        }

        public void Write(string objKey, string objValue, int timestamp = -1) {
            if (IsMaster) WriteMaster(objKey, objValue);
            WriteSlave(objKey, objValue, timestamp);
        }

        public void WriteMaster(string objKey, string objValue) {
            int timeStamp = Objects[objKey].WriteNext(objValue);
            WriteSlaveRequest request = new WriteSlaveRequest {
                PartitionId = _partitionId,
                ObjectId = objKey,
                ObjectValue = objValue,
                Timestamp = timeStamp
            };
            foreach (SlaveServiceClient slave in slaveServiceClients) {
                slave.WriteSlaveAsync(request);
            }
        }

        public void WriteSlave(string objKey, string objValue, int timestamp) {
            Objects[objKey].Write(objValue, timestamp);
        }
    }

    public class ObjectInfo {
        private int _timestampCounter = 0;
        private object _monitor = new object();
        private string _objectValue;

        public string Read() {
            lock (_monitor) {
                return _objectValue;
            }

            /* 
                client will need a cache if this _timestampcounter is lower than the
                timestamp counter of the client. To be considered later
            */
        }

        public void Write(string newValue, int timestampCounter) {
            //int observed = -1;
            //while (true) {
            //    observed = _timestampCounter;
            //    if (timestampCounter <= observed) return;
            //    if (Interlocked.CompareExchange(ref _timestampCounter, timestampCounter, observed) == timestampCounter) {  _objectValue = newValue; return;}
            //}

            lock (_monitor) {
                if (timestampCounter <= _timestampCounter) return;
                _objectValue = newValue;
                _timestampCounter = timestampCounter;
            }
        }

        public int WriteNext(string newValue) {
            lock (_monitor) {
                _objectValue = newValue;
                return ++_timestampCounter;
            }
        }
    }
}