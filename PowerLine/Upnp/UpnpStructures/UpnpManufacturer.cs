using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine.Upnp.UpnpCustomPackets
{
    public class UpnpManufacturer
    {
        public string Name { get; set; }
        public string Url { get; set; } = null;

        public UpnpManufacturer(string name)
        {
            Name = name;
        }
    }
}
