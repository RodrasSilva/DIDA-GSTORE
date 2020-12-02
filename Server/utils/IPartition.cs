using System;
using System.Collections.Generic;
using System.Text;

namespace Server.utils {
    public interface IPartition {
        public bool IsMaster { get; set; }
    }
}