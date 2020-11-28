public class AdvancedServerObjectInfo {
    private readonly object _monitor = new object();
    private string _objectValue;
    private int _timestampCounter;

    public string Read() {
        lock (_monitor){
            return _objectValue;
        }

        /* 
            client will need a cache if this _timestampcounter is lower than the
            timestamp counter of the client. To be considered later
        */
    }

    public void Write(string newValue, int timestampCounter) {

        lock (_monitor){
            if (timestampCounter <= _timestampCounter) return;
            _objectValue = newValue;
            _timestampCounter = timestampCounter;
        }
    }

    public int WriteNext(string newValue) {
        lock (_monitor){
            _objectValue = newValue;
            return ++_timestampCounter;
        }
    }
}