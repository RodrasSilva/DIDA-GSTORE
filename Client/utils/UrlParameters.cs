using System;

namespace Client.utils {
    public class UrlParameters
    {

        private UrlParameters(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;
        }

        public string Hostname { get; }
        public int Port { get; }

        public static UrlParameters From(string url)
        {
            //Console.WriteLine(url);
            var parsedUrl = url.Split(':');
            if(parsedUrl.Length == 2){
                string hostname = parsedUrl[0];
                var server = int.Parse(parsedUrl[1]);
                return new UrlParameters(hostname, server);
            }
            else if(parsedUrl.Length == 3)
            {
                string hostname = parsedUrl[1];
                hostname = hostname.Substring(2);
                var server = int.Parse(parsedUrl[2]);
                return new UrlParameters(hostname, server);
            }
            throw new Exception("Bad format on url: " + url);

            
        }
    }
}