using PowerLine.Upnp.UpnpCustomPackets;
using PowerLine.Upnp.UpnpInternal;
using PowerLine.Upnp.UpnpStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine.Upnp.UpnpControl
{
    public class UpnpRootDevice : UpnpBaseDevice
    {
        public static readonly string DeviceSchema = $"urn:schemas-upnp-org:device-{UpnpEngine.UpnpVersionMajor}-{UpnpEngine.UpnpVersonMinor}";
        public UpnpRootDevice(Guid id, string friendlyName, UpnpNtDeviceType deviceType, UpnpManufacturer manufacturer, UpnpModel model) : base(new UpnpFragmentWithRoot(deviceType, new UpnpNt(id), new UpnpNt()), friendlyName, manufacturer, model) { }

    }
}
