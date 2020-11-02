using System;

public class BaseServerObjectInfo
{
    private object _monitor = new object();
    private string _objectValue;

    public string Read()
    {
        lock (_monitor)
        {
            return _objectValue;
        }
    }

    public void Write(String newValue)
    {
        lock (_monitor)
        {
            _objectValue = newValue;
        }
    }
}