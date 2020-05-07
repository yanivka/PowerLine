using System;
using System.Collections.Generic;
using System.Net;


namespace PowerLine
{
    public abstract class PowerLineHandler
    {
        public readonly string HttpMethod;

        public PowerLineHandler(string httpMethod)
        {
            this.HttpMethod = httpMethod;
        }
        internal PowerLineEndPointExecutionResult SafeHandleRequest( PowerLineEndPoint endpoint, int index, string[] requestPath, PowerLineContext context)
        {
            try
            {
                this.HandleRequest(context);
                return new PowerLineEndPointExecutionResult(context, endpoint, this);
            } 
            catch(Exception ex)
            {
                return new PowerLineEndPointExecutionResult(context, endpoint, this, ex);
            }
        }
           
        public abstract void HandleRequest(PowerLineContext context);
    }
}
