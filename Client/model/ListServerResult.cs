namespace Client.model{
    public class ListServerResult{
        public ListServerResult(string objectId, string objectValue, bool isMaster){
            ObjectId = objectId;
            ObjectValue = objectValue;
            IsMaster = IsMaster;
        }

        public string ObjectId{ get; }
        public string ObjectValue{ get; }

        public bool IsMaster{ get; }
    }
}