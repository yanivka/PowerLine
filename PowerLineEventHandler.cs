using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PowerLine
{

    public enum PowerLineEventHandlerResponseType :int
    {
        EventRaied = 600,
        EventSubscribed = 601,
        EventUnsubscribed = 602,
        EventNotFound = 603,
        EventAlreadySubscribed = 604,
        EventFaildSubscription = 605
    }
    internal class PowerLineEventHandler : PowerLineHandler
    {
        public readonly PowerLineServer Server;
        public PowerLineEventHandler(PowerLineServer server): base("PUT")
        {
            this.Server = server;
        }
        public override async Task HandleRequest(PowerLineContext context)
        {
            if (!context.IsWebSocket)
            {
                context.SetResponse(400);
                context.SetResponseHttpString("Only websockets support this kind of endpoints");
                return;
            }
            else
            {
                JObject obj = await context.ReadResponsePayloadAsJson();
                string eventName = obj.GetValue<string>(new string[] { "payload", "name" });
                lock (context.WebsocketClient.eventsLock)
                {
                    if(context.WebsocketClient.events.ContainsKey(eventName))
                    {
                        context.SetResponse((int)PowerLineEventHandlerResponseType.EventAlreadySubscribed);
                        context.SetResponseHttpString("You are already subscribed to this event");
                        return;
                    }
                }
                PowerLineEvent currentEvent = Server.GetEvent(eventName);
                if(currentEvent == null)
                {
                    context.SetResponse((int)PowerLineEventHandlerResponseType.EventNotFound);
                    context.SetResponseHttpString("Given event was not found");
                    return;
                }
                if(Server.innerSubscribeEvent(context.WebsocketClient, currentEvent))
                {
                    context.SetResponse((int)PowerLineEventHandlerResponseType.EventSubscribed);
                    context.SetResponseHttpString("You are subscribed to the event");
                    return;
                }
                else
                {
                    context.SetResponse((int)PowerLineEventHandlerResponseType.EventFaildSubscription);
                    context.SetResponseHttpString("Something went wrong while susbscirbing to event");
                    return;
                }
            }

            
        }

        public static PowerLineEndPoint GetEndPoint(PowerLineServer server) => new PowerLineEndPoint("Event", new PowerLineHandler[] { new PowerLineEventHandler(server) });
    }
}
