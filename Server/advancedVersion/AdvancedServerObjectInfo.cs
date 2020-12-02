using System;
using System.Threading;

public class AdvancedServerObjectInfo {
    public struct ObjectVal {
        public string value;
        public int timestampCounter;
    }

    private ObjectVal _objectVal;
    //private string _objectValue;
    //private int _timestampCounter;

    public AdvancedServerObjectInfo(string value) {
        _objectVal = new ObjectVal()
            {value = value, timestampCounter = -1};
    }

    public ObjectVal Read() {
        lock (this) {
            return _objectVal;
        }
    }

    public void Write(string newValue, int timestampCounter) {
        lock (this) {
            if (timestampCounter <= _objectVal.timestampCounter) return;
            _objectVal.value = newValue;
            _objectVal.timestampCounter = timestampCounter;
        }
    }

    public int WriteNext(string newValue) {
        lock (this) {
            _objectVal.value = newValue;
            return ++_objectVal.timestampCounter;
        }
    }
}