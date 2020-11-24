using PowerLine.Upnp.UpnpInternal;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PowerLine.Upnp
{
    public class UpnpEngineDevice : IDisposable
    {
        public readonly UpnpBaseDevice Device;
        private DateTime LastNotification;
        public readonly UpnpEngine Engine;

        public UpnpEngineDevice(UpnpBaseDevice device, UpnpEngine engine)
        {
            Device = device;
            LastNotification = DateTime.MinValue;
            Engine = engine;

            this.Device._enableChanged += OnDeviceEnableChange;
        }

        private void OnDeviceEnableChange(object sender, bool state)
        {
            this.LastNotification = DateTime.MinValue;
            this.Engine.RefreshNotifyer();
        }
          
        public void Notifiy()
        {
            this.LastNotification = DateTime.UtcNow;
            Task.Run(() => this.Engine.InvokeDeviceNotification(this));
        }
        public DateTime GetNextNotification() => (this.LastNotification == DateTime.MinValue) ? DateTime.UtcNow : this.LastNotification.Add(Device.NotificationInterval);
        public DateTime GetLastNotification() => this.LastNotification;

        public string GetHttpDevice(UpnpPlaceHolder placeHolder = null)
        {
            MemoryStream memory = new MemoryStream();
            this.Device.GetXmlDocument(placeHolder).Save(memory, SaveOptions.None);
            memory.Position = 0;
            return System.Text.Encoding.UTF8.GetString(memory.ToArray());
        }
        public string GetHttpDeviceMime()
        {
            return PowerLineContextContentType.Xml.GetMimeType();
        }

        public void Dispose()
        {
            this.Device._enableChanged -= OnDeviceEnableChange;
        }
    }
}
