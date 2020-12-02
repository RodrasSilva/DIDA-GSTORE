using System.Threading;

public class BaseServerObjectInfo {
    public ManualResetEvent _lock;
    private string _objectValue;

    public BaseServerObjectInfo(string value) {
        _lock = new ManualResetEvent(false);
        _objectValue = value;
    }


    //assumes that it was called with the reader lock
    public string Read() {
        return _objectValue;
    }

    // assumes that it was called with the writter lock
    public void Write(string newValue) {
        _objectValue = newValue;
    }
}