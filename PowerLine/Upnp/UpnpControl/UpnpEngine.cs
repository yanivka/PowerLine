using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using PowerLine.Upnp.UpnpControl;
using PowerLine.Upnp.UpnpCustomHttp;
using PowerLine.Upnp.UpnpCustomPackets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PowerLine.Upnp
{
    public class UpnpEngine
    {
        
        public const int UpnpVersionMajor = 1;
        public const int UpnpVersonMinor = 0;

        public static readonly Random random = new Random();
        public static readonly XElement UpnpVersion = new XElement("specVersion", new XElement("major", UpnpVersionMajor), new XElement("minor", UpnpVersonMinor));

        private Socket socketReference = null;
        private Thread mainRunner;
        private Thread notifierRuner;
        private CancellationTokenSource cancelToken;
        private EventWaitHandle waitHandle;
        private object startStopLock;

        public const int SSDP_PORT = 1900;
        public const string SSDP_IP = "239.255.255.250";

        private List<UpnpEngineDevice> devices;
        private object deviceLock;

        private PowerLineServer Server;

        public UpnpEngine(PowerLineServer server)
        {
            this.Server = server;
            this.waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            this.deviceLock = new object();
            this.startStopLock = new object();
            this.devices = new List<UpnpEngineDevice>();
        }

        public void Start()
        {
            lock(this.startStopLock)
            {
                this.UnsafeStart();
            }
        }
        private void UnsafeStart()
        {
            this.UnsafeStop();
            this.waitHandle.Reset();
            this.cancelToken = new CancellationTokenSource();
            this.mainRunner = new Thread(this.Runner);
            this.mainRunner.IsBackground = true;
            this.mainRunner.Start();
            this.notifierRuner = new Thread(this.RunnerNotifier);
            this.notifierRuner.IsBackground = true;
            this.notifierRuner.Start();
        }
        public void Stop()
        {
            lock (this.startStopLock)
            {
                this.UnsafeStop();
            }
        }
        private void UnsafeStop()
        {
            if(this.cancelToken != null) this.cancelToken.Cancel();
            if(this.socketReference  != null)
            {
                try
                {
                    socketReference.Close();
                }catch{}
            }
            this.waitHandle.Set();
            if (this.mainRunner != null) this.mainRunner.Join();
            if (this.notifierRuner != null) this.notifierRuner.Join();
        }
        internal void RefreshNotifyer()
        {
            this.waitHandle.Set();
        }
       public UpnpRootDevice AddDevice(UpnpRootDevice device)
        {
            lock(this.deviceLock)
            {
                UpnpEngineDevice engineDevice = new UpnpEngineDevice(device, this);
                this.devices.Add(engineDevice);
            }
            this.RefreshNotifyer();
            return device;
        }
        private void Runner()
        {
            using (var mSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                IPEndPoint bindIp = new IPEndPoint(IPAddress.Any, SSDP_PORT);
                mSendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(SSDP_IP), IPAddress.Any));
                mSendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 255);
                mSendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                mSendSocket.MulticastLoopback = true;
                mSendSocket.Bind(bindIp);
                byte[] buffer = new byte[UInt16.MaxValue];
                while(!this.cancelToken.IsCancellationRequested)
                {
                   
                    EndPoint all = new IPEndPoint(IPAddress.Any, SSDP_PORT);
                    SocketFlags flags = SocketFlags.None;
                    int packetSize = mSendSocket.ReceiveMessageFrom(buffer, 0, buffer.Length, ref flags, ref all, out IPPacketInformation infomation);
                    this.OnRunnerPacket(buffer, packetSize, (IPEndPoint)all, new IPEndPoint(infomation.Address, SSDP_PORT), GetInterfaceLocalIp(((IPEndPoint)all).Address));
                }   
            }
        }
        private void RunnerNotifier()
        {
            while(!this.cancelToken.IsCancellationRequested)
            {
                UpnpEngineDevice selectedDevice = this.GetMinNextNotification();
                if(selectedDevice == null)
                {
                    this.waitHandle.WaitOne();
                }
                else
                {
                    DateTime lastNotification = selectedDevice.GetNextNotification();
                    if (lastNotification < DateTime.UtcNow.AddSeconds(5))
                    {
                        selectedDevice.Notifiy();
                    }
                    else
                    {
                        this.waitHandle.WaitOne(lastNotification - DateTime.UtcNow);
                    }
                }
            }
        }
        private UpnpEngineDevice GetMinNextNotification()
        {
            UpnpEngineDevice minDevice = null;
            lock(this.deviceLock)
            {
               foreach(UpnpEngineDevice device in this.devices)
                {
                    if(minDevice == null || (minDevice.GetLastNotification() > device.GetLastNotification()))
                    {
                        minDevice = device;
                    }
                }
            }
            return minDevice;
        }
        internal void InvokeDeviceNotification(UpnpEngineDevice device)
        {
            for (int i = 0; i < 2; i++)
            {
                this.Brodcast((ipAddress) =>
                {
                    return new UpnpSsdpNotify(
                        (int)device.Device.NotificationInterval.TotalSeconds,
                        SSDP_IP,
                        SSDP_PORT,
                        this.GetDeviceHttpAddress(device, ipAddress),
                        device.Device.UpnpRoot.ToString(),
                        (device.Device.Enable) ? UpnpSsdpNotify.NotifyAlive : UpnpSsdpNotify.NotifyByeBye,
                        device.Device.UpnpRoot.GetUsn(device.Device.Id.id).ToString()).Encode();
                });
                this.Brodcast((ipAddress) =>
                {
                    return new UpnpSsdpNotify(
                        (int)device.Device.NotificationInterval.TotalSeconds,
                        SSDP_IP,
                        SSDP_PORT,
                        this.GetDeviceHttpAddress(device, ipAddress),
                        device.Device.Id.ToString(),
                        (device.Device.Enable) ? UpnpSsdpNotify.NotifyAlive : UpnpSsdpNotify.NotifyByeBye,
                        device.Device.Id.GetUsn(device.Device.Id.id).ToString()).Encode();
                });
                this.Brodcast((ipAddress) =>
                {
                    return new UpnpSsdpNotify(
                        (int)device.Device.NotificationInterval.TotalSeconds,
                        SSDP_IP,
                        SSDP_PORT,
                       this.GetDeviceHttpAddress(device, ipAddress),
                        device.Device.UpnpType.ToString(),
                        (device.Device.Enable) ? UpnpSsdpNotify.NotifyAlive : UpnpSsdpNotify.NotifyByeBye,
                        device.Device.UpnpType.GetUsn(device.Device.Id.id).ToString()).Encode();
                });
                if (!device.Device.Enable) return;
                System.Threading.Thread.Sleep(2500);
            }
        }
        private List<UpnpEngineDevice> SearchDevices(UpnpNt nt)
        {
            List<UpnpEngineDevice> foundDevices = new List<UpnpEngineDevice>();
            lock(this.deviceLock)
            {
                foreach(UpnpEngineDevice device in this.devices)
                {
                    if (nt == null || device.Device.DeviceNt.IsMatch(nt)) foundDevices.Add(device);
                }
            }
            return foundDevices;
        }
        private void OnRunnerPacket(byte[] packet, int size, IPEndPoint remoteEndpoint, IPEndPoint localEndpoint, IPAddress  localInterfaceIpOfRemoteAddress)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(packet, 0, size);
            stream.Position = 0;
            HttpRequest req = HttpRequest.Decode(stream);
            Task.Run(() => this.OnRunnerRequest(req, remoteEndpoint,localEndpoint, localInterfaceIpOfRemoteAddress), this.cancelToken.Token);
        }
        private void OnRunnerRequest(HttpRequest request, IPEndPoint remoteEndpoint, IPEndPoint localEndpoint, IPAddress localInterfaceIpOfRemoteAddress)
        {
            if(request.Method == "M-SEARCH")
            {
                HttpHeader requestSubType = request.Headers.Get("MAN", true);
                if (requestSubType == null) throw new Exception("Invaild m-search quary (missing MAN header)");
                if(requestSubType.Value.Replace("\"", "").ToLower() == "ssdp:discover")
                {
                    OnRunnerDiscovery(request, remoteEndpoint, localEndpoint, localInterfaceIpOfRemoteAddress);
                }
            }
        }
        private void OnRunnerDiscovery(HttpRequest request, IPEndPoint remoteEndpoint, IPEndPoint localEndpoint, IPAddress localInterfaceIpOfRemoteAddress)
        {
            HttpHeader searchFor = request.Headers.Get("ST", true);
            if(searchFor == null) throw new Exception("Invaild m-search quary (missing ST header)");

            

            UpnpNt searchItem = UpnpNt.Parse(searchFor.Value);

            Console.WriteLine($"Ssdp discovery search for {searchItem.ToString()} detected from {remoteEndpoint.ToString()} to {localEndpoint.ToString()} can response using {localInterfaceIpOfRemoteAddress?.ToString()}");


            List<UpnpEngineDevice> foundDevices = this.SearchDevices(searchItem);
            foreach(UpnpEngineDevice singleFoundDevice in foundDevices)
            {
                OnRunnerDisocveryForDevice(request, singleFoundDevice, searchItem, remoteEndpoint, localEndpoint, localInterfaceIpOfRemoteAddress);
            }
            
        }
        private void OnRunnerDisocveryForDevice(HttpRequest request, UpnpEngineDevice device, UpnpNt searchId, IPEndPoint remoteEndpoint, IPEndPoint localEndpoint, IPAddress localInterfaceIpOfRemoteEndpoint)
        {
            Console.WriteLine("Sending response for disconvery");

            HttpHeader sendDelay = request.Headers.Get("MX", true);
            if (sendDelay != null && int.TryParse(sendDelay.Value, out int maxDelay))
            {
                int currentSleep = random.Next(1, maxDelay * 1000);
                System.Threading.Thread.Sleep(currentSleep);
            }
            UpnpSsdpQueryResponse response = new UpnpSsdpQueryResponse(
                (int)device.Device.NotificationInterval.TotalSeconds,
                searchId, device.Device.IdUsn, this.GetDeviceHttpAddress(device, localInterfaceIpOfRemoteEndpoint));

            this.Send(response, remoteEndpoint);
        }
        private string GetDeviceHttpAddress(UpnpEngineDevice device, IPAddress localInterfaceIpOfRemoteEndpoint = null)
        {
            return $"{this.GetPowerLineHttpAddress(localInterfaceIpOfRemoteEndpoint)}UPnP/{device.Device.Id.id.ToString()}.xml";
        }
        private string GetPowerLineHttpAddress(IPAddress localInterfaceIpOfRemoteEndpoint = null)
        {
            if(this.Server.BindAddress == IPAddress.Any)
            {
                if(localInterfaceIpOfRemoteEndpoint == null)
                {
                    return PowerLineServer.BuildBindUrl(Dns.GetHostName(), this.Server.BindPort);
                }
                else
                {
                    return PowerLineServer.BuildBindUrl(localInterfaceIpOfRemoteEndpoint.ToString(), this.Server.BindPort);
                }
            }
            else 
            {
                return this.Server.BuildBindUrl();
            }
        }
        private void Send(HttpRequest res, IPEndPoint target) => this.Send(res.Encode(), target);
        private void Send(HttpResponse res, IPEndPoint target) => this.Send(res.Encode(), target);
        private void Send(byte[] data, IPEndPoint target)
        {
            using (var mSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                Console.WriteLine($"Sending response to {target.ToString()}");
                mSendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                mSendSocket.Bind(new IPEndPoint(IPAddress.Any, SSDP_PORT));
                mSendSocket.SendTo(data, target);
            }
        }
        private void Brodcast(HttpRequest res) => this.Brodcast(res.Encode());
        private void Brodcast(HttpResponse res) {
            this.Brodcast((ipAddress) => {
                UpnpPlaceHolder placeHolder = new UpnpPlaceHolder(ipAddress.ToString(), SSDP_PORT, SSDP_IP, SSDP_PORT);
                return res.Encode(placeHolder);
            });
        }
        private void Brodcast(Func<IPAddress, byte[]> getData)
        {
            IPAddress GroupIp = IPAddress.Parse(SSDP_IP);
            foreach (NetworkInterface netInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().Where((item) => item.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
            {
                foreach (UnicastIPAddressInformation ip in netInterface.GetIPProperties().UnicastAddresses.Where((item) => item.Address.AddressFamily == AddressFamily.InterNetwork))
                {
                    byte[] data = getData(ip.Address);
                    using (var mSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                    {
                        mSendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(GroupIp, ip.Address));
                        mSendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 255);
                        mSendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        mSendSocket.MulticastLoopback = true;
                        mSendSocket.Bind(new IPEndPoint(ip.Address, SSDP_PORT));
                        mSendSocket.SendTo(data, new IPEndPoint(GroupIp, SSDP_PORT));
                    }
                }
            }
        }
        private void Brodcast(byte[] data) => this.Brodcast((ip) => data);
        private UpnpEngineDevice GetDevice(Guid id)
        {
            lock(this.deviceLock)
            {
                foreach(UpnpEngineDevice device in this.devices)
                {
                    if (device.Device.Id.id == id) return device;
                }
            }
            return null;
        }
        internal async Task OnHttpRequest(PowerLineContext context)
        {
            Console.WriteLine($"Upnp xml was requeted by {context.RemoteAddress}:{context.RemotePort.ToString()}");
            string fullName = Path.GetFileName(context.ReqeustUri.LocalPath);
            string itemName = Path.GetFileNameWithoutExtension(fullName);
            string itemExtention = Path.GetExtension(fullName);
            if(itemExtention == ".xml")
            {
                if (Guid.TryParse(itemName, out Guid result))
                {
                    OnHttpRequestItem(context, result, itemExtention);
                }
                else
                {
                    context.SetResponse(404, "Unable to detect given element, invaild guid");
                }
            }
            else
            {
                context.SetResponse(404, "Unable to detect given element, invaild mimetype");
            }
        }
        private void OnHttpRequestItem(PowerLineContext context, Guid itemName, string itemExtention)
        {
            UpnpEngineDevice device = this.GetDevice(itemName);
            if(device == null)
            {
                context.SetResponse(404, "Unable to detect given element, device not found");
            }
            else
            {
                UpnpPlaceHolder placeHolder = new UpnpPlaceHolder(context.LocalAddress, context.LocalPort, context.RemoteAddress, context.RemotePort);
                string rawDeviceUpnpXml = device.GetHttpDevice(placeHolder);
                context.SetRepsonseContentType(device.GetHttpDeviceMime());
                context.SetResponse(200, rawDeviceUpnpXml,false);
            }
        }
        private static bool CheckMask(IPAddress address, IPAddress mask, IPAddress target)
        {
            if (mask == null)
                return false;

            var ba = address.GetAddressBytes();
            var bm = mask.GetAddressBytes();
            var bb = target.GetAddressBytes();

            if (ba.Length != bm.Length || bm.Length != bb.Length)
                return false;

            for (var i = 0; i < ba.Length; i++)
            {
                int m = bm[i];

                int a = ba[i] & m;
                int b = bb[i] & m;

                if (a != b)
                    return false;
            }

            return true;
        }
        private static IPAddress GetInterfaceLocalIp(IPAddress lanIpAddress)
        {
            foreach (NetworkInterface netInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().Where((item) => item.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
            {
                foreach (UnicastIPAddressInformation ip in netInterface.GetIPProperties().UnicastAddresses.Where((item) => item.Address.AddressFamily == AddressFamily.InterNetwork))
                {
                    if (CheckMask(ip.Address, ip.IPv4Mask, lanIpAddress)) return ip.Address;
                }
            }
            return null;
        }


        public static XDocument CreateUpnpDocument(string upnpSchema, XElement root)
        {
            root = root.FixNamespace(XNamespace.Get(upnpSchema), new XNode[] { UpnpEngine.UpnpVersion });
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
            return doc;
        }

    }
}
