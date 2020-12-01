using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Server.utils
{
    public class FreezeUtilities {

        private static ManualResetEvent mre = new ManualResetEvent(true);
        private static bool discard = false;

        public void WaitForUnfreeze() {
            mre.WaitOne();
        }

        public void Unfreeze() {
            if (discard) discard = false;
            mre.Set();
        }

        public void Freeze() {
            mre.Reset();
        }

        public void Discard() {
            discard = true;
        }

        public bool IsToDiscard() {
            return discard;
        }
    }
}
