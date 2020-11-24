using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine.Upnp.UpnpCustomHttp
{
    public class UpnpSsdpNotify : HttpRequest
    {
        public const string NotifyAlive = "ssdp:alive";
        public const string NotifyByeBye = "ssdp:byebye";

        public UpnpSsdpNotify(int maxAge,string host, int port, string location, string nt, string nts, string usn)
            :base("NOTIFY", "*", "HTTP/1.1",
                 new HttpHeaders(new HttpHeader[] { 
                    new HttpHeader("HOST", $"{host}:{port}"),
                    new HttpHeader("DATE", DateTime.Now.ToString("o")),
                    new HttpHeader("CACHE-CONTROL", $"max-age={maxAge}"),
                    new HttpHeader("LOCATION", location),
                    new HttpHeader("NT", nt),
                    new HttpHeader("NTS", nts),
                    new HttpHeader("USN", usn),
                    new HttpHeader("SERVER", "PowerLine/1 UPnP/1.0 PowerLineUpnp/1"),
                 })){}
    }
}
