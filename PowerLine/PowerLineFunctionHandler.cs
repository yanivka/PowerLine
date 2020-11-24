using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerLine
{
    internal class PowerLineFunctionHandler : PowerLineHandler
    {
        private readonly Func<PowerLineContext, Task> HandlerFunction;
        public PowerLineFunctionHandler(string httpMethod, Func<PowerLineContext, Task> handlerFunction) :base(httpMethod)
        {
            this.HandlerFunction = handlerFunction;
        }
        public override Task HandleRequest(PowerLineContext context) => this.HandlerFunction(context);
    }
}
