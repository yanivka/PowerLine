using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace PowerLine.Upnp
{
    public class HttpRequest
    {

        public readonly string Method;
        public readonly string Url;
        public readonly string Text;
        public readonly HttpHeaders Headers;

        public HttpRequest(string method, string url, string text, HttpHeaders headers)
        {
            Method = method;
            Url = url;
            Text = text;
            Headers = headers;
        }

        public static HttpRequest Decode(byte[] data) => HttpRequest.Decode(new MemoryStream(data));
        public static HttpRequest Decode(Stream data)
        {
            List<string> builder = new List<string>();         
            using (StreamReader reader = new StreamReader(data))
            {
                string currentLine = reader.ReadLine();
                while(currentLine != null && currentLine != "")
                {
                    builder.Add(currentLine);
                    currentLine = reader.ReadLine();
                }
            }
            if (!builder.Any()) throw new Exception("Error: Unable to parse custom http packets");
            string[] httpMainLine = builder[0].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (httpMainLine.Length <= 2) throw new Exception("Error: Unable to parse custom http packets (Short Main Line)");
            string httpMethod = httpMainLine[0];
            string httpUrl = httpMainLine[1];
            string httpText = string.Join(" ", httpMainLine.Skip(2));
            return new HttpRequest(httpMethod, httpUrl, httpText, HttpHeaders.LoadFromLines(builder.Skip(1)));

        }
        public byte[] Encode()
        {
            MemoryStream stream = new MemoryStream();
            this.Encode(stream);
            stream.Position = 0;
            return stream.ToArray();
        }
        public void Encode(Stream memoryStream, bool flushStream = true)
        {
            memoryStream.WriteLine($"{this.Method} {this.Url} {this.Text}");
            Array.ForEach(this.Headers.Items(), (singleHeader) => memoryStream.WriteLine(singleHeader.ToString()));
            memoryStream.WriteLine("");
            if (flushStream) memoryStream.Flush();

        }

    }
}
;