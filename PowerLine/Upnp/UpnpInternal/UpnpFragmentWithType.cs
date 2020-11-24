using PowerLine.Upnp.UpnpCustomPackets;
using PowerLine.Upnp.UpnpStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine.Upnp.UpnpInternal
{
    public class UpnpFragmentWithType : IUpnpTypeMatchable
    {

        public readonly UpnpNt UpnpType;

        internal UpnpFragmentWithType(UpnpNt upnpType)
        {
            UpnpType = upnpType;
        }

        public virtual bool IsMatch(UpnpNt searchObject) => this.UpnpType.Equal(searchObject);
        public virtual UpnpNt GetByNtType(UpnpNt.NtType ntType) => (this.UpnpType.Type == ntType) ? this.UpnpType : null;
    }
}
