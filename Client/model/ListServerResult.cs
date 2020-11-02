namespace Client.model {
    public class ListServerResult {
        public string ObjectId { get; private set; }
        public string ObjectValue { get; private set; }

        public bool IsMaster { get; private set; }

        public ListServerResult(string objectId, string objectValue, bool isMaster) {
            ObjectId = objectId;
            ObjectValue = objectValue;
            IsMaster = IsMaster;
        }
    }
}