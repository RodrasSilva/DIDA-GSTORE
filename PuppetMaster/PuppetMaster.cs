using Client.utils;
using DIDA_GSTORE.grpcService;

namespace PuppetMasterMain {
    public class PuppetMaster {
        private static GrpcProcessService grpcProcessService;

        public static void Main(string[] args) {
            //grpcProcessService = new GrpcProcessService("localhost", 5001);
            grpcProcessService = urlToProcessService("localhost:5001");
            var puppetMasterDomain = new PuppetMasterDomain();
            puppetMasterDomain.Start(args, grpcProcessService);
        }

        public static GrpcProcessService urlToProcessService(string url) 
        {
            UrlParameters urlParams = UrlParameters.From(url);
            return new GrpcProcessService(urlParams.Hostname, urlParams.Port);
        }

        public static GrpcNodeService urlToNodeService(string url)
        {
            UrlParameters urlParams = UrlParameters.From(url);
            return new GrpcNodeService(urlParams.Hostname, urlParams.Port);
        }
    }
}