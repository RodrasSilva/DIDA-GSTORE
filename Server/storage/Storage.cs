using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Server.storage
{
    public class Storage
    {
        // Partition object map -> string key = object key and string data => object value
        public Dictionary<string, ObjectInfo> Objects
        {
            get; private set;
        }
        public Storage() {
            Objects = new Dictionary<string, ObjectInfo>();
        }

    }
    public class ObjectInfo {

        public string objectValue; 
    }

    //<"2", "OK">
    // Write (2) "OK 2"
    // master lock request => slaves
    // slaves lockRequest() => Objects["2"].lock
    // master unlock request => slaves
    // slaves unlockRequest() => Objects["2"] = "OK 2" ; Objects["2"].unlock

}
