using PowerLine.Upnp.UpnpCustomPackets;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine.Upnp.UpnpInternal
{
    public class UpnpFragmentWithId: UpnpFragmentWithType, IUpnpTypeMatchable
    {

        public readonly UpnpNt UpnpId;

        internal UpnpFragmentWithId(UpnpNt upnpType, UpnpNt upnpId): base(upnpType)
        {
            UpnpId = upnpId;
        }

        public override bool IsMatch(UpnpNt searchObject) => (this.UpnpId.Equal(searchObject)) ? true : base.IsMatch(searchObject);

        public override UpnpNt GetByNtType(UpnpNt.NtType ntType) => (this.UpnpId.Type == ntType) ? this.UpnpId : base.GetByNtType(ntType);
    }
}
