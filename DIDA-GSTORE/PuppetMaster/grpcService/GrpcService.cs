using System;
using Grpc.Core;

namespace DIDA_GSTORE.grpcService {
    public class GrpcService {
        private const string ServerIp = "127.0.0.1";
        private const string ServerPort = "50051";

        private static readonly string ServerAddress = String.Format("{0}:{1}", ServerIp, ServerPort);
        /*
        private static readonly Channel Channel = new Channel(ServerAddress, ChannelCredentials.Insecure);
        private static DidaServiceClient _client = null;

        public DidaServiceClient Service {
            get {
                if (_client != null) return _client;
                return _client = new DidaService.DidaServiceClient(Channel);
            }
        }
        */
    }
}