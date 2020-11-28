using System.Threading;

public class AdvancedServerObjectInfo {
    public struct ObjectVal
    {
        public string value;
        public int timestampCounter;

    }

    public ManualResetEvent _lock;
    private ObjectVal _objectVal;
    //private string _objectValue;
    //private int _timestampCounter;

    public AdvancedServerObjectInfo(string value)
    {
        _lock = new ManualResetEvent(false);
        _objectVal = new ObjectVal() 
            { value = value, timestampCounter = 0 };
    }

    public ObjectVal Read()
    {
        return _objectVal;
    }

    public ObjectVal Read(string clientObjectValue, int clientTimestamp) {
        if(clientTimestamp > _objectVal.timestampCounter)
        {
            _objectVal.value = clientObjectValue;
            _objectVal.timestampCounter = clientTimestamp;
        }
        return _objectVal;

        /* 
            client will need a cache if this _timestampcounter is lower than the
            timestamp counter of the client. To be considered later
        */
    }

    public void Write(string newValue, int timestampCounter) {

        if (timestampCounter <= _objectVal.timestampCounter) return;
        _objectVal.value = newValue;
        _objectVal.timestampCounter = timestampCounter;
    }

    public int WriteNext(string newValue) {
        _objectVal.value = newValue;
        return ++_objectVal.timestampCounter;
    }
}