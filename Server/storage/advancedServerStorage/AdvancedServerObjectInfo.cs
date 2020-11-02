public class AdvancedServerObjectInfo {
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