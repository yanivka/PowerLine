using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine.Upnp.UpnpCustomPackets
{
    public class UpnpModel
    {
        public string Name { get; set; } = null;
        public string Number { get; set; } = null;
        public string Description { get; set; } = null;
        public string Url { get; set; } = null;

        public UpnpModel(string name)
        {
            Name = name;
        }
    }
}
