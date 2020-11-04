using System;

namespace Client.utils{
    public class UrlParameters{
        private const int HostnamePosition = 1;
        private const int PortPosition = 2;

        private UrlParameters(string hostname, int port){
            Hostname = hostname;
            Port = port;
        }

        public string Hostname{ get; }
        public int Port{ get; }

        public static UrlParameters From(string url){
            //Console.WriteLine("Parsing url "+url+"  in server");
            var parsedUrl = url.Split(':');
            if (parsedUrl.Length != 3) throw new Exception("Bad format on url: " + url);

            var hostname = parsedUrl[HostnamePosition];
            hostname = hostname.Substring(2);
            var server = int.Parse(parsedUrl[PortPosition]);

            //Console.WriteLine("Pased url " + url + "  in server with host = "+hostname+" ; port = "+server);
            return new UrlParameters(hostname, server);
        }
    }
}