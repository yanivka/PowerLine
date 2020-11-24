using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PowerLine
{
    public enum PowerLineHandleMethod
    {
        GET,
        HEAD,
        POST,
        PUT,
        DELETE,
        CONNECT,
        OPTIONS,
        TRACE,
        PATCH
    }
    public abstract class PowerLineHandler
    {
        public readonly string HttpMethod;

       
        public PowerLineHandler(string httpMethod)
        {
            this.HttpMethod = httpMethod;
        }
        public PowerLineHandler(PowerLineHandleMethod httpMethod) : this(httpMethod.ToString()){}
        
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

        public PowerLineHandler AsMethod(PowerLineHandleMethod httpMethod) => this.AsMethod(httpMethod.ToString());
        public PowerLineHandler AsMethod(string httpMethod)
        {
            return new PowerLineShadowHandler(httpMethod, (PowerLineHandler)this.MemberwiseClone());
        }


        public static PowerLineHandler Create(string httpMethod, Func<PowerLineContext, Task> mainFunction) => new PowerLineFunctionHandler(httpMethod, mainFunction);
        public static PowerLineHandler Create(PowerLineHandleMethod httpMethod, Func<PowerLineContext, Task> mainFunction) => Create(httpMethod.ToString(), mainFunction);
        public static PowerLineHandler Create(string httpMethod, Action<PowerLineContext> mainFunction) => new PowerLineVoidFunctionHandler(httpMethod, mainFunction);
        public static PowerLineHandler Create(PowerLineHandleMethod httpMethod, Action<PowerLineContext> mainFunction) => Create(httpMethod.ToString(), mainFunction);


        public static implicit operator PowerLineHandler(Func<PowerLineContext, Task> mainFunction)
        {
            return Create(PowerLineHandleMethod.GET, mainFunction);
        }
        public static implicit operator PowerLineHandler(Action<PowerLineContext> mainFunction)
        {
            return Create(PowerLineHandleMethod.GET, mainFunction);
        }
    }
}
