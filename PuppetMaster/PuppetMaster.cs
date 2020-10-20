using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DIDA_GSTORE.commands;
using DIDA_GSTORE.grpcService;

namespace PuppetMasterMain
{
    public class PuppetMaster
    {
        private static GrpcService grpcService;

        public static void Main(string[] args)
        {
            grpcService = new GrpcService("localhost", 5001);

            PuppetMasterDomain puppetMasterDomain = new PuppetMasterDomain();
            puppetMasterDomain.Start(args, grpcService);
        }
    }
}
