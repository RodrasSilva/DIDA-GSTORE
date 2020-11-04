using System.Collections.Generic;

namespace Client.model{
    public class ListGlobalResult{
        public ListGlobalResult(List<ListGlobalResultIdentifier> identifiers){
            Identifiers = identifiers;
        }

        public List<ListGlobalResultIdentifier> Identifiers{ get; }
    }

    public class ListGlobalResultIdentifier{
        public ListGlobalResultIdentifier(string partitionId, string objectId){
            PartitionId = partitionId;
            ObjectId = objectId;
        }

        public string PartitionId{ get; }
        public string ObjectId{ get; }
    }
}