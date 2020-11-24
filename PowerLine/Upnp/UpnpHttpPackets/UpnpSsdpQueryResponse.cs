using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine.Upnp.UpnpCustomPackets
{
    public class UpnpSsdpQueryResponse : HttpResponse
    {
        public UpnpSsdpQueryResponse(int maxAge, UpnpNt searchObject, UpnpUsn targetDevice, string location) :
            base("HTTP/1.1", 200, "OK",
                new HttpHeaders(
                    new HttpHeader[] {
                        new HttpHeader("CACHE-CONTROL", $"max-age={maxAge}"),
                        new HttpHeader("DATE", DateTime.Now.ToString("o")),
                        new HttpHeader("ST", searchObject.ToString()),
                        new HttpHeader("USN", targetDevice.ToString()),
                        new HttpHeader("EXT", ""),
                        new HttpHeader("SERVER", "PowerLine/1 UPnP/1.0 PowerLineUpnp/1"),
                        new HttpHeader("LOCATION", location),

                    }))
        {

        }
    }
}
