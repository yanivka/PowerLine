using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine.Upnp.UpnpStructures
{
    public class UpnpNtDeviceType : UpnpNtType
    {
        public UpnpNtDeviceType(string domain, string ItemType, string ItemVersion) : base(domain, ItemType, ItemVersion, true) { }
        public UpnpNtDeviceType(string ItemType, string ItemVersion) : base(ItemType, ItemVersion, true) { }
        public UpnpNtDeviceType(string domain, string customDevice, string ItemType, string ItemVersion) : base(domain, customDevice, ItemType, ItemVersion) { }
    }
}
