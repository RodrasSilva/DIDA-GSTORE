using System.Collections.Generic;

namespace Client.model {
    public class ListGlobalResult {
        public List<ListGlobalResultIdentifier> Identifiers { get; private set; }

        public ListGlobalResult(List<ListGlobalResultIdentifier> identifiers) {
            Identifiers = identifiers;
        }
    }

    public class ListGlobalResultIdentifier {
        public string PartitionId { get; private set; }
        public string ObjectId { get; private set; }

        public ListGlobalResultIdentifier(string partitionId, string objectId) {
            PartitionId = partitionId;
            ObjectId = objectId;
        }
    }
}