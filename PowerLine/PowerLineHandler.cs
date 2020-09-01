using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PowerLine
{
    public abstract class PowerLineHandler
    {
        public readonly string HttpMethod;

        public PowerLineHandler(string httpMethod)
        {
            this.HttpMethod = httpMethod;
        }
        internal async Task<PowerLineEndPointExecutionResult> SafeHandleRequestAsync(PowerLineEndPoint endpoint, int index, string[] requestPath, PowerLineContext context)
        {
            try
            {
                context.PathIndex = index;
                await this.HandleRequest(context);
                return new PowerLineEndPointExecutionResult(context, endpoint, this);
            } 
            catch(Exception ex)
            {
                return new PowerLineEndPointExecutionResult(context, endpoint, this, ex);
            }
        }
           
        public abstract Task HandleRequest(PowerLineContext context);
    }
}
