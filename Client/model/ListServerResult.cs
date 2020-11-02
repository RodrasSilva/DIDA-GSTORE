namespace Client.model
{
    public class ListServerResult
    {
        public string ObjectValue { get; private set; }
        public bool IsMaster { get; private set; }

        public ListServerResult(string objectValue, bool isMaster)
        {
            ObjectValue = objectValue;
            IsMaster = IsMaster;
        }
    }
}