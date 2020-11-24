using PowerLine.Upnp.UpnpCustomPackets;
using PowerLine.Upnp.UpnpInternal;
using PowerLine.Upnp.UpnpStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PowerLine.Upnp
{
    public class UpnpDevice : UpnpBaseDevice
    {
        public static readonly string DeviceSchema = $"urn:schemas-upnp-org:device-{UpnpEngine.UpnpVersionMajor}-{UpnpEngine.UpnpVersonMinor}";
        public UpnpDevice(Guid id, string friendlyName, UpnpNtDeviceType deviceType, UpnpManufacturer manufacturer, UpnpModel model) : base(new UpnpFragmentWithId(deviceType, new UpnpNt(id)), friendlyName, manufacturer, model){}
    }
}
