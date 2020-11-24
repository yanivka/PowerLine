using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PowerLine.Upnp
{
    class PowerLineUpnpHttpHandle : PowerLineHandler
    {
        public readonly UpnpEngine Engine;

        public PowerLineUpnpHttpHandle(UpnpEngine engine) : base("GET")
        {
            Engine = engine;
        }

        public override async Task HandleRequest(PowerLineContext context) => await this.Engine.OnHttpRequest(context);

        public static PowerLineEndPoint GetEndpoint(UpnpEngine engine) =>
            new PowerLineEndPoint("UPnP", null,
                new PowerLineEndPoint[] {
                    new PowerLineEndPoint("DyanmicUpnpHttpHandler",
                        new PowerLineHandler[] { new PowerLineUpnpHttpHandle(engine) } , true) });
    }
}
