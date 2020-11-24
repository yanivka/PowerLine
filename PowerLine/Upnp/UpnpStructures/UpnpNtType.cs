using PowerLine.Upnp.UpnpCustomPackets;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine.Upnp.UpnpStructures
{
    public class UpnpNtType : UpnpNt
    {
        public UpnpNtType(string domain, string ItemType, string ItemVersion, bool IsDevice) : base(domain, ItemType, ItemVersion, IsDevice) { }
        public UpnpNtType(string ItemType, string ItemVersion, bool IsDevice) : base(ItemType, ItemVersion, IsDevice) { }
        public UpnpNtType(string domain, string customDevice, string ItemType, string ItemVersion) : base(domain, customDevice, ItemType, ItemVersion) { }
    }
}
