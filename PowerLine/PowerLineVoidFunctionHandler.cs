using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerLine
{
    internal class PowerLineVoidFunctionHandler : PowerLineHandler
    {
        private readonly Action<PowerLineContext> HandlerFunction;
        public PowerLineVoidFunctionHandler(string httpMethod, Action<PowerLineContext> handlerFunction) : base(httpMethod)
        {
            this.HandlerFunction = handlerFunction;
        }
        public override Task HandleRequest(PowerLineContext context)
        {
            this.HandlerFunction(context);
            return Task.FromResult(0);
        }
    }
}
