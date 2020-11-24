using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine.Upnp
{
    public class HttpHeader
    {
        public string Name;
        public string Value;

        public HttpHeader(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public static HttpHeader LoadFromLine(string httpLine)
        {
            int index = httpLine.IndexOf(":");
            if (index == -1) throw new Exception("Error: Unable to decode http headers");
            return new HttpHeader(httpLine.Substring(0, index).Trim(), httpLine.Substring(index + 1).Trim());
        }

        public override string ToString() => this.ToString(null);
        public string ToString(UpnpPlaceHolder placeHolder)
        {
            return (placeHolder == null) ? $"{this.Name}: {this.Value}" : $"{this.Name}: {placeHolder.Parse(this.Value)}"; 
        }
    }
}
