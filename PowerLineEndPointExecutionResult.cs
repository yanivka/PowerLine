using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine
{
    public enum PowerLinExecutionResultType
    {
        OK = 0,
        EndPointNotFound = 1,
        HttpMethodNotFound = 2,
        HandlerException = 3
    }
    public class PowerLineEndPointExecutionResult
    {
        public readonly PowerLinExecutionResultType ResultType;
        public readonly Exception Exception;
        public readonly PowerLineContext Context;
        public readonly PowerLineEndPoint EndPoint;
        public readonly PowerLineHandler Handler;

        public PowerLineEndPointExecutionResult(PowerLinExecutionResultType resultType, PowerLineContext context, Exception exception, PowerLineEndPoint endpoint, PowerLineHandler handler)
        {
            this.ResultType = resultType;
            this.Context = context;
            this.Exception = exception;
            this.EndPoint = endpoint;
            this.Handler = handler;
        }
        public PowerLineEndPointExecutionResult(PowerLineContext context, PowerLinExecutionResultType resultType, PowerLineEndPoint endPoint) : this(resultType, context, null, endPoint, null){}
        public PowerLineEndPointExecutionResult(PowerLineContext context, PowerLinExecutionResultType resultType, PowerLineEndPoint endPoint, PowerLineHandler handler) : this(resultType, context, null, endPoint, handler) { }
        public PowerLineEndPointExecutionResult(PowerLineContext context, PowerLineEndPoint endPoint, PowerLineHandler handler, Exception exception) : this(PowerLinExecutionResultType.HandlerException, context, exception, endPoint, handler) { }
        public PowerLineEndPointExecutionResult(PowerLineContext context, PowerLineEndPoint endPoint, PowerLineHandler handler) : this(PowerLinExecutionResultType.OK, context, null, endPoint, handler) { }
    }
}
