using PowerLine.Upnp.UpnpCustomPackets;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine.Upnp.UpnpInternal
{
    public class UpnpFragmentWithRoot : UpnpFragmentWithId , IUpnpTypeMatchable
    {

        public readonly UpnpNt UpnpRoot;

        internal UpnpFragmentWithRoot(UpnpNt upnpType, UpnpNt upnpId, UpnpNt upnpRoot):base(upnpType, upnpId)
        {
            UpnpRoot = upnpRoot;
        }

        public override bool IsMatch(UpnpNt searchObject) => (this.UpnpRoot.Equal(searchObject)) ? true : base.IsMatch(searchObject);
        public override UpnpNt GetByNtType(UpnpNt.NtType ntType) => (this.UpnpRoot.Type == ntType) ? this.UpnpRoot : base.GetByNtType(ntType);
    }
}
