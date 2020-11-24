using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerLine
{
    internal class PowerLineShadowHandler : PowerLineHandler
    {

        PowerLineHandler BaseHandler;
        public PowerLineShadowHandler(string httpMethod, PowerLineHandler baseHandler ):base(httpMethod)
        {
            this.BaseHandler = baseHandler;
        }
        public override Task HandleRequest(PowerLineContext context) => BaseHandler.HandleRequest(context);
    }
}
