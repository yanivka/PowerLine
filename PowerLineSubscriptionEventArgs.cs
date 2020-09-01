using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine
{
    public class PowerLineSubscriptionEventArgs
    {
        public readonly PowerLineWebsocketClient Client;
        internal bool cancel;


        public PowerLineSubscriptionEventArgs(PowerLineWebsocketClient client)
        {
            this.Client = client;
            this.cancel = false;
        }

        public void Cancel()
        {
            this.cancel = true;
        }
    }
}
