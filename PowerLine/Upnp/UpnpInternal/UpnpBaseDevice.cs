using PowerLine.Upnp.UpnpCustomPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PowerLine.Upnp.UpnpInternal
{
    public class UpnpBaseDevice
    {
        internal IUpnpTypeMatchable DeviceNt;

        internal event EventHandler<bool> _enableChanged;

        private bool _enable = true;
        public bool Enable { 
            get
            { 
                return this._enable; 
            } 
            set
            { 
                this._enable = value;
                this._enableChanged?.Invoke(this, this._enable);
            } 
        }

        public UpnpNt Id { get { return DeviceNt.GetUpnpId(); } }
        public UpnpUsn IdUsn { get { return new UpnpUsn(DeviceNt.GetUpnpId().id); } }
        public UpnpNt UpnpType { get { return DeviceNt.GetUpnpType(); } }
        public UpnpNt UpnpRoot { get { return DeviceNt.GetUpnpRoot(); } }
        public bool IsRoot { get { return this.UpnpRoot != null; } }

        public string FriendlyName { get; set; }
        public string SerialNumber { get; set; } = null;
        public string PresentationURL { get; set; } = null;
        public UpnpModel Model { get; set; }
        public UpnpManufacturer Manufacturer { get; set; }
        public List<UpnpIcon> Icons { get; set; } = new List<UpnpIcon>();
        public List<UpnpService> Services { get; set; } = new List<UpnpService>();
        public List<UpnpDevice> Devices { get; set; } = new List<UpnpDevice>();
        public string UPC { get; set; } = null;

        public TimeSpan NotificationInterval { get; set; } = new TimeSpan(0, 30, 0);


        internal UpnpBaseDevice(IUpnpTypeMatchable deviceNt, string friendlyName, UpnpManufacturer manufacturer, UpnpModel model)
        {
            this.DeviceNt = deviceNt;
            this.FriendlyName = friendlyName;
            this.Manufacturer = manufacturer;
            this.Model = model;
        }
        public XDocument GetXmlDocument(UpnpPlaceHolder urlPlaceHolders = null)
        {
            XElement root = new XElement("root");
            root.Add(this.GetXml(urlPlaceHolders));
            return UpnpEngine.CreateUpnpDocument(UpnpDevice.DeviceSchema, root);
        }
        private string InvokePlaceHolder(string baseString, UpnpPlaceHolder urlPlaceHolders)
        {
            return (urlPlaceHolders == null) ? baseString : urlPlaceHolders.Parse(baseString);
        }   
        private XElement GetXml(UpnpPlaceHolder urlPlaceHolders = null)
        {
            XElement deviceObject = new XElement("device");

            if (this.UpnpType != null) deviceObject.Add(new XElement("deviceType", this.InvokePlaceHolder(this.UpnpType.ToString(), urlPlaceHolders)));
            if (this.FriendlyName != null) deviceObject.Add(new XElement("friendlyName", this.InvokePlaceHolder(this.FriendlyName, urlPlaceHolders)));

            if (this.Manufacturer != null)
            {
                if (this.Manufacturer.Name != null) deviceObject.Add(new XElement("manufacturer", this.InvokePlaceHolder(this.Manufacturer.Name, urlPlaceHolders)));
                if (this.Manufacturer.Url != null) deviceObject.Add(new XElement("manufacturerURL", this.InvokePlaceHolder(this.Manufacturer.Url, urlPlaceHolders)));
            }

            if (this.Model != null)
            {
                if (this.Model.Description != null) deviceObject.Add(new XElement("modelDescription", this.InvokePlaceHolder(this.Model.Description, urlPlaceHolders)));
                if (this.Model.Name != null) deviceObject.Add(new XElement("modelName", this.InvokePlaceHolder(this.Model.Name, urlPlaceHolders)));
                if (this.Model.Number != null) deviceObject.Add(new XElement("modelNumber", this.InvokePlaceHolder(this.Model.Number, urlPlaceHolders)));
                if (this.Model.Number != null) deviceObject.Add(new XElement("modelURL", this.InvokePlaceHolder(this.Model.Url, urlPlaceHolders)));
            }

            if (this.SerialNumber != null) deviceObject.Add(new XElement("serialNumber", this.InvokePlaceHolder(this.SerialNumber, urlPlaceHolders)));
            deviceObject.Add(new XElement("UDN", this.InvokePlaceHolder(this.IdUsn.ToString(), urlPlaceHolders)));
            if (this.UPC != null) deviceObject.Add(new XElement("UPC", this.InvokePlaceHolder(this.UPC, urlPlaceHolders)));
            if (this.Icons != null && this.Icons.Any())
            {
                XElement iconList = new XElement("iconList");
                foreach (UpnpIcon icon in this.Icons)
                {
                    iconList.Add(icon.GetXml());
                }
                deviceObject.Add(iconList);
            }
            if (this.Services != null && this.Services.Any())
            {
                XElement serviceList = new XElement("serviceList");
                foreach (UpnpService service in this.Services)
                {
                    serviceList.Add(service.GetXml());
                }
                deviceObject.Add(serviceList);
            }
            if (this.Devices != null && this.Devices.Any())
            {
                XElement deviceList = new XElement("deviceList");
                foreach (UpnpBaseDevice device in this.Devices)
                {
                    deviceList.Add(device.GetXml());
                }
                deviceObject.Add(deviceList);
            }
            if (this.PresentationURL != null) deviceObject.Add(new XElement("presentationURL", this.InvokePlaceHolder(this.PresentationURL, urlPlaceHolders)));
            return deviceObject;
        }
    }
}
