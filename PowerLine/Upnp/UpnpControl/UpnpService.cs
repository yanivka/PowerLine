using PowerLine.Upnp.UpnpCustomPackets;
using PowerLine.Upnp.UpnpInternal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace PowerLine.Upnp
{
    public class UpnpService 
    {
        private IUpnpTypeMatchable ServiceNt;
        public UpnpNt Id { get { return ServiceNt.GetUpnpId(); } }
        public UpnpNt UpnpType { get { return ServiceNt.GetUpnpType(); } }

        public UpnpService(IUpnpTypeMatchable serviceNt, string sspDUrl, string controlUrl, string eventSubUrl)
        {
            this.ServiceNt = serviceNt;
            this.SspDUrl = sspDUrl;
            this.ControlUrl = controlUrl;
            this.EventSubUrl = eventSubUrl;
        }

        public string SspDUrl { get; set; }
        public string ControlUrl { get; set; }
        public string EventSubUrl { get; set; }

        public XElement GetEmbeddedXml()
        {
            XElement service = new XElement("service");
            if (this.UpnpType != null) service.Add(new XElement("serviceType", this.UpnpType.ToString()));
            if (this.Id != null)  service.Add(new XElement("serviceId", this.UpnpType.ToServiceId(this.Id.id)));
            if (this.SspDUrl != null) service.Add(new XElement("SCPDURL", this.SspDUrl));
            if (this.ControlUrl != null) service.Add(new XElement("controlURL", this.ControlUrl));
            if (this.EventSubUrl != null) service.Add(new XElement("eventSubURL", this.EventSubUrl));
            return service;
        }
        public XElement GetXml()
        {
            return null;
        }
       
    }
}
