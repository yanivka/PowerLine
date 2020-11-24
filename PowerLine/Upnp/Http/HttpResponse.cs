using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PowerLine.Upnp
{
    public class HttpResponse
    {
        public readonly string HttpVersion;
        public readonly int HttpCode;
        public readonly string HttpText;
        public readonly HttpHeaders Headers;

        public HttpResponse(string httpVersion, int httpCode, string httpText, HttpHeaders headers)
        {
            HttpVersion = httpVersion;
            HttpCode = httpCode;
            HttpText = httpText;
            Headers = headers;
        }

        public static HttpResponse Decode(byte[] data) => HttpResponse.Decode(new MemoryStream(data));
        public static HttpResponse Decode(Stream data)
        {
            List<string> builder = new List<string>();
            using (StreamReader reader = new StreamReader(data))
            {
                string currentLine = reader.ReadLine();
                while (currentLine != null && currentLine != "")
                {
                    builder.Add(currentLine);
                    currentLine = reader.ReadLine();
                }
            }
            if (!builder.Any()) throw new Exception("Error: Unable to parse custom http packets");
            string[] httpMainLine = builder[0].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (httpMainLine.Length <= 2) throw new Exception("Error: Unable to parse custom http packets (Short Main Line)");
            string httpVersion = httpMainLine[0];
            string httpCode = httpMainLine[1];
            string httpText = string.Join(" ", httpMainLine.Skip(2));
            return new HttpResponse(httpVersion, int.Parse(httpCode), httpText, HttpHeaders.LoadFromLines(builder.Skip(1)));

        }
        public byte[] Encode(UpnpPlaceHolder placeHolder = null)
        {
            MemoryStream stream = new MemoryStream();
            this.Encode(stream, placeHolder);
            stream.Position = 0;
            return stream.ToArray();
        }
        public void Encode(Stream memoryStream, UpnpPlaceHolder placeHolder = null,  bool flushStream = true)
        {
            memoryStream.WriteLine($"{this.HttpVersion} {this.HttpCode} {this.HttpText}");
            Array.ForEach(this.Headers.Items(), (singleHeader) => memoryStream.WriteLine(singleHeader.ToString()));
            memoryStream.WriteLine("");
            if (flushStream) memoryStream.Flush();

        }
    }
}
