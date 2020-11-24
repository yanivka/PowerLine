using PowerLine.Upnp.UpnpCustomPackets;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine.Upnp.UpnpInternal
{
    public interface IUpnpTypeMatchable
    {
        bool IsMatch(UpnpNt searchObject);
        UpnpNt GetByNtType(UpnpNt.NtType ntType);
    }
    public static class IUpnpTypeMatchableExtentions
    {
        public static UpnpNt GetUpnpType(this IUpnpTypeMatchable item) => 
            item.GetByNtType(UpnpNt.NtType.DeviceType) ??
            item.GetByNtType(UpnpNt.NtType.ServiceType) ??
            item.GetByNtType(UpnpNt.NtType.DomainDeviceType) ??
            item.GetByNtType(UpnpNt.NtType.DomainServiceType) ??
            item.GetByNtType(UpnpNt.NtType.CustomType) ??
            item.GetByNtType(UpnpNt.NtType.DomainCustomType);

        public static UpnpNt GetUpnpId(this IUpnpTypeMatchable item) => item.GetByNtType(UpnpNt.NtType.Guid);
        public static UpnpNt GetUpnpRoot(this IUpnpTypeMatchable item) => item.GetByNtType(UpnpNt.NtType.RootDevice);


    }
}
