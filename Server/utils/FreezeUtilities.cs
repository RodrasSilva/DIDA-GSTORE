using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Server.utils
{
    public class FreezeUtilities {

        private static ManualResetEvent mre = new ManualResetEvent(true);

        public void WaitForUnfreeze(){
            mre.WaitOne();
        }


        public void Unfreeze() {
            mre.Set();
        }

        public void Freeze(){
            mre.Reset();
        }

    }
}
