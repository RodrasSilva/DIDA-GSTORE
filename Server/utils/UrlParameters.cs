using System;

namespace Client.utils
{
    public class UrlParameters
    {
        private const int HostnamePosition = 0;
        private const int PortPosition = 0;

        public string Hostname { get; }
        public int Port { get; }

        private UrlParameters(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;
        }

        public static UrlParameters From(string url)
        {
            string[] parsedUrl = url.Split(':');
            if (parsedUrl.Length != 2)
            {
                throw new Exception("Bad format on url: " + parsedUrl);
            }

            string hostname = parsedUrl[HostnamePosition];
            int server = int.Parse(parsedUrl[PortPosition]);
            return new UrlParameters(hostname, server);
        }
    }
}