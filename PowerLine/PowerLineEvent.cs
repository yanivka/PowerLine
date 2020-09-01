using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerLine
{
    public class PowerLineEvent
    {
        public readonly string Name;

        internal List<PowerLineWebsocketClient> clients;
        internal SemaphoreSlim clientsLock;

        public event EventHandler<PowerLineSubscriptionEventArgs> Subscription;
        
        public PowerLineEvent(string Name)
        {
            this.clients = new List<PowerLineWebsocketClient>();
            this.clientsLock = new SemaphoreSlim(1, 1);
            this.Name = Name;
        }


        internal bool SubscribeWithCheck(PowerLineWebsocketClient client)
        {
            PowerLineSubscriptionEventArgs args = new PowerLineSubscriptionEventArgs(client);
            Subscription?.Invoke(this, args);
            if(args.cancel)
            {
                return false;
            }
            else
            {
                this.SubscribeClient(client);
                return true;
            }
        }
        public async Task RaiseEventAsync(JToken message)
        {
            await this.clientsLock.WaitAsync();
            try
            {
                await Task.WhenAll(this.clients.Select((item) => item.RaiseEventAsync(this, message)));
            }
            finally
            {
                this.clientsLock.Release();
            }
           
        }
        public async Task RaiseEventAsync(JToken message, PowerLineWebsocketClient[] DontIncludeClients)
        {
            await this.clientsLock.WaitAsync();
            try
            {
                await Task.WhenAll(this.clients.Where((item) => !DontIncludeClients.Contains(item)).Select((item) => item.RaiseEventAsync(this, message)));
            }
            finally
            {
                this.clientsLock.Release();
            }

        }
        internal void SubscribeClient(PowerLineWebsocketClient websocket)
        {
            this.clientsLock.WaitAsync();
            try
            {
                this.clients.Add(websocket);
            }
            finally
            {
                this.clientsLock.Release();
            }
        }
        internal bool UnsubscribeClient(PowerLineWebsocketClient websocket)
        {
            this.clientsLock.WaitAsync();
            try
            {
                return this.clients.Remove(websocket);
            }
            finally
            {
                this.clientsLock.Release();
            }
        }
    }
}
