using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DIDA_GSTORE.commands;
using DIDA_GSTORE.grpcService;

namespace PuppetMasterMain {
    public class PuppetMaster {
        private static GrpcProcessService grpcProcessService;

        public static void Main(string[] args) {
            //grpcProcessService = new GrpcProcessService("localhost", 5001);
            grpcProcessService = null;
            PuppetMasterDomain puppetMasterDomain = new PuppetMasterDomain();
            puppetMasterDomain.Start(args, grpcProcessService);
        }

        public static GrpcProcessService urlToProcessService(string url) {
            return new GrpcProcessService(url, 5001);
        }

        public static GrpcNodeService urlToNodeService(string url) {
            return new GrpcNodeService(url, 5001);
        }
    }
}