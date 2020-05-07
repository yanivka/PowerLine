using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace PowerLine
{
    public class PowerLineContext
    {
        public readonly HttpListenerRequest request;
        public readonly HttpListenerResponse response;
        public readonly HttpListenerContext context;
        public readonly int PathIndex;
        public readonly string[] Path;

        public PowerLineContext(HttpListenerContext context, int pathIndex, string[] path)
        {
            this.context = context;
            this.request = context.Request;
            this.response = context.Response;
            this.PathIndex = pathIndex;
            this.Path = path;
        }
    }
}
