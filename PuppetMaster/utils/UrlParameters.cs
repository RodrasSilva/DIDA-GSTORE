﻿using System;

namespace Client.utils {
    public class UrlParameters {
        private const int HostnamePosition = 0;
        private const int PortPosition = 1;

        private UrlParameters(string hostname, int port) {
            Hostname = hostname;
            Port = port;
        }

        public string Hostname { get; }
        public int Port { get; }

        public static UrlParameters From(string url) {
            Console.WriteLine(url);
            var parsedUrl = url.Split(":");
            if (parsedUrl.Length != 2) throw new Exception("Bad format on url: " + url);

            var hostname = parsedUrl[HostnamePosition];
            var server = int.Parse(parsedUrl[PortPosition]);
            return new UrlParameters(hostname, server);
        }
    }
}