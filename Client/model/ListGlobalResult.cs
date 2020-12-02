using System.Collections.Generic;

namespace Client.model {
    public class ListGlobalResult {
        public ListGlobalResult(List<ListGlobalResultIdentifier> identifiers) {
            Identifiers = identifiers;
        }

        public List<ListGlobalResultIdentifier> Identifiers { get; }
    }

    public class ListGlobalResultIdentifier {
        public ListGlobalResultIdentifier(string partitionId, List<string> objectIds) {
            PartitionId = partitionId;
            ObjectIds = objectIds;
        }

        public string PartitionId { get; }
        public List<string> ObjectIds { get; }
    }
}