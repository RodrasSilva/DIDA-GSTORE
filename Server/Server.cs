using System;
using System.Linq;
using System.Threading;
using Client.utils;
using DIDA_GSTORE.ServerService;
using Grpc.Core;
using Grpc.Net.Client;
using Server.advancedVersion;
using Server.baseVersion;
using Server.grpcService;
using Server.utils;

namespace ServerDomain{
    public class Server
    {
        private const bool UseBaseVersion = false;

        public static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("You gave: " + string.Join(" ", args) +
                                  ". Usage is: Server <id> <url> <minDelay> <maxDelay>");
                return;
            }
            var _serverId = args[0];
            var _serverUrl = args[1];
            var _minDelay = float.Parse(args[2]);
            var _maxDelay = float.Parse(args[3]);

            if (UseBaseVersion)
            {
#pragma warning disable CS0162 // Unreachable code detected
                new BaseServer(_minDelay, _maxDelay, _serverId, _serverUrl).Run();
#pragma warning restore CS0162 // Unreachable code detected
            }
            else
            {
#pragma warning disable CS0162 // Unreachable code detected
                new AdvancedServer(_minDelay, _maxDelay, _serverId, _serverUrl).Run();
#pragma warning restore CS0162 // Unreachable code detected
            }

        }
    }
}