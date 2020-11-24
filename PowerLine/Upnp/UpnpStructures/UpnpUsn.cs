using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine.Upnp.UpnpCustomPackets
{
    public class UpnpUsn
    {
        public readonly Guid id;
        public readonly UpnpNt Nt;

        public UpnpUsn(Guid id) : this(id, null) { }
        public UpnpUsn(Guid id, UpnpNt nt)
        {
            this.id = id;
            Nt = nt;
        }

        public static UpnpUsn Parse(string usn)
        {
            UpnpNt nt = null;
            int SplitIndex = usn.IndexOf("::");
            if(SplitIndex != -1)
            {
                nt = UpnpNt.Parse(usn.Substring(SplitIndex + 1));
                usn = usn.Substring(0, SplitIndex);
            }
            string[] usnParts = usn.Trim().Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (usnParts.Length != 2) throw new Exception("Unable to parse upnp usn, invaild amount");
            if (usnParts[0] == "uuid") throw new Exception("Unable to parse upnp usn, invaild starting");
            if(Guid.TryParse(usnParts[1], out Guid usnId))
            {
                return new UpnpUsn(usnId, nt);
            }
            else
            {
                throw new Exception("Unable to parse upnp usn, invaild guid");
            }
        }

        public string GetDeviceUsn() => this.ToString();
        public string GetServiceUsn(UpnpService service) =>  $"urn:{((service))}:serviceId:{this.id.ToString()}";

        public override string ToString()
        {
            return $"uuid:{this.id.ToString()}{(Nt == null || Nt.Type == UpnpNt.NtType.Guid ? "" : $"::{Nt.ToString()}")}";
        }
        public bool Equal(UpnpUsn usn)
        {
            return (this.id == usn.id && ((usn.Nt == null && this.Nt == null) || (usn.Nt != null && this.Nt != null && this.Nt.Equal(usn.Nt)))) ? true : false;
        }
    }
}
