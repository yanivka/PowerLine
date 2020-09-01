using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PowerLine
{
    public class PowerLineEndPoint
    {
        public readonly string EndPointName;
        public readonly bool Dynamic;
        private Dictionary<string, PowerLineHandler> handlers;
        private Dictionary<string, PowerLineEndPoint> childEndPoints;

        public PowerLineEndPoint(string endPointName, bool dynamic=false):this(endPointName, null, null, dynamic){}
        public PowerLineEndPoint(string endPointName, IEnumerable<PowerLineHandler> handlers,  bool dynamic = false) : this(endPointName, handlers, null, dynamic){}
        public PowerLineEndPoint(string endPointName, IEnumerable<PowerLineHandler> handlers, IEnumerable<PowerLineEndPoint> endpoints, bool dynamic = false)
        {
            this.EndPointName = endPointName;
            this.Dynamic = dynamic;
            this.handlers = (handlers == null) ? new Dictionary<string, PowerLineHandler>() : new Dictionary<string, PowerLineHandler>(handlers.Select((item) => new KeyValuePair<string, PowerLineHandler>(item.HttpMethod, item)));
            this.childEndPoints = (endpoints == null) ? new Dictionary<string, PowerLineEndPoint>() : new Dictionary<string, PowerLineEndPoint>(endpoints.Select((item) => new KeyValuePair<string, PowerLineEndPoint>(item.EndPointName, item)));          
        }

        public bool ContainsHandler(string handlerMethod) => this.handlers.ContainsKey(handlerMethod);
        public bool ContainsEndPoint(string endpointName) => this.childEndPoints.ContainsKey(endpointName);
        public PowerLineHandler AddHandler(PowerLineHandler handler)
        {
            this.handlers.Add(handler.HttpMethod, handler);
            return handler;
        }
        public PowerLineHandler GetHandler(string handlerMethod)
        {
            if(this.handlers.TryGetValue(handlerMethod, out PowerLineHandler currentHandler))
            {
                return currentHandler;
            }
            else
            {
                return null;
            }
        }
        public PowerLineEndPoint AddEndPoint(PowerLineEndPoint endpoint)
        {
            this.childEndPoints.Add(endpoint.EndPointName, endpoint);
            return endpoint;
        }
        public PowerLineEndPoint GetEndPoint(string endpointNAME)
        {
            if (this.childEndPoints.TryGetValue(EndPointName, out PowerLineEndPoint currentHandler))
            {
                return currentHandler;
            }
            else
            {
                return null;
            }
        }
        internal async Task<PowerLineEndPointExecutionResult> OnSelfRequestAsync(int index, string[] requestPath, PowerLineContext context)
        {
            if(this.handlers.TryGetValue(context.RequestMethod, out PowerLineHandler handler))
            {
                return await handler.SafeHandleRequestAsync(this, index, requestPath, context);
            }
            else
            {
                return new PowerLineEndPointExecutionResult(context, PowerLinExecutionResultType.HttpMethodNotFound, this);
            }
        }
        internal async Task<PowerLineEndPointExecutionResult> OnRequestAsync(int index, string[] requestPath, PowerLineContext context)
        {
            if(index >= requestPath.Length)
            {
                return await this.OnSelfRequestAsync(index, requestPath, context);
            }
            else
            {
                if(this.childEndPoints.TryGetValue(requestPath[index], out PowerLineEndPoint endpoint))
                {
                    return await endpoint.OnRequestAsync(index + 1, requestPath, context);
                }
                else if(this.Dynamic)
                {
                    return await this.OnSelfRequestAsync(index, requestPath, context);
                }
                else
                {
                    return new PowerLineEndPointExecutionResult(context, PowerLinExecutionResultType.EndPointNotFound, this);
                }
            }
        }

        public virtual bool VerifyDynamicEndpoint(Uri requestLine) => true;
    }
}
