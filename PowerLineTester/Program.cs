using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using PowerLine;

namespace PowerLineTester
{
    class Program
    {
        static int Main(string[] args)
        {
            PowerLineServer server = new PowerLineServer(IPAddress.Parse("127.0.0.1"), 2000);
            server.Start();
            Console.WriteLine("Server is running");

            PowerLineEndPoint endpoint = new PowerLineEndPoint("", new PowerLineHandler[] { new NormalMethod() });
            server.AddEndpoint(endpoint);

            server.AddHandler("test", (context) => context.SetResponse(200, "This page is working")); 

            server.Wait();
            Console.WriteLine("Done");
            Console.ReadKey();
            return 0;
        }
    }


    class NormalMethod : PowerLineHandler
    {
        public NormalMethod():base("GET"){}
        public override Task HandleRequest(PowerLineContext context)
        {
            string html = "<html><head><title>Main Title</title></head><body><h1>Running..</h1></body></html>";
            byte[] htmlEncoded = System.Text.Encoding.UTF8.GetBytes(html);
            Stream mainStream = context.response.OutputStream;
            mainStream.Write(htmlEncoded);
            return Task.FromResult(0);
        }
    }
}
