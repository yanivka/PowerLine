using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PowerLine.Upnp
{
    public class UpnpPlaceHolder
    {
        public const string LocalIp = "%PlaceHolder-LOCAL-IP%";
        public const string LocalPort = "%PlaceHolder-LOCAL-PORT%";
        public const string RemoteIp = "%PlaceHolder-REMOTE-IP%";
        public const string RemotePort = "%PlaceHolder-REMOTE-PORT%";
        public const string UpnpMajor = "%PlaceHolder-UPNP-MAJOR%";
        public const string UpnpMinor = "%PlaceHolder-UPNP-MINOR%";
        public const string Date = "%PlaceHolder-LOCAL-DATE%";


        public Dictionary<string, string> hashtable;

        public UpnpPlaceHolder(Dictionary<string, string> hashtable)
        {
            this.hashtable = (hashtable == null) ? new Dictionary<string, string>() : hashtable;
            this.hashtable.Add(UpnpMajor, UpnpEngine.UpnpVersionMajor.ToString());
            this.hashtable.Add(UpnpMinor, UpnpEngine.UpnpVersonMinor.ToString());
            this.hashtable.Add(Date, DateTime.UtcNow.ToString("o"));
        }
        public UpnpPlaceHolder(string localIp, int localPort, string remoteIp, int remotePort) : this(new Dictionary<string, string>(new KeyValuePair<string, string>[] { 
            new KeyValuePair<string, string>(UpnpPlaceHolder.LocalIp, localIp) ,
            new KeyValuePair<string, string>(UpnpPlaceHolder.LocalPort, localPort.ToString()),
            new KeyValuePair<string, string>(UpnpPlaceHolder.RemoteIp, remoteIp),
            new KeyValuePair<string, string>(UpnpPlaceHolder.RemotePort, remotePort.ToString())
        })){}

        public string Parse(string str)
        {
            foreach(KeyValuePair<string, string> item in this.hashtable)
            {
                str = str.Replace(item.Key, item.Value);
            }
            return str;
        }
    }
}
