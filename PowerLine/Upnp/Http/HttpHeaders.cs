using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace PowerLine.Upnp
{
    public class HttpHeaders
    {

        private Dictionary<string, HttpHeader> _headers;
        private Dictionary<string, HttpHeader> _lowcaseHeaders;

        public HttpHeaders(Dictionary<string, HttpHeader> headers = null)
        {
            _headers = headers ?? new Dictionary<string, HttpHeader>();
            this._lowcaseHeaders = new Dictionary<string, HttpHeader>(this._headers.Select((item) => new KeyValuePair<string, HttpHeader>(item.Key.ToLower(), item.Value)));
        }
        public HttpHeaders(IEnumerable<HttpHeader> httpHeaders) : this(new Dictionary<string, HttpHeader>(httpHeaders.Select((header) => new KeyValuePair<string, HttpHeader>(header.Name, header))))
        {}

        public static HttpHeaders LoadFromLines(IEnumerable<string> lines) => new HttpHeaders(lines.Select((singleLine) => HttpHeader.LoadFromLine(singleLine)));
        public HttpHeader[] Items() => this._headers.Values.ToArray();
        public string[] Names() => this._headers.Keys.ToArray();
        public void Add(HttpHeader header) => this.Set(header.Name, header.Value);
        public void Add(string name, string value) => this.Set(name, value);
        public void Set(string name, string value)
        {
            if(this._headers.ContainsKey(name))
            {
                this._headers[name].Value = value;
            }
            else
            {
                this._headers[name] = new HttpHeader(name, value);
            }
            string lowerName = name.ToLower();
            if (this._lowcaseHeaders.ContainsKey(lowerName))
            {
                this._lowcaseHeaders[lowerName].Value = value;
            }
            else
            {
                this._lowcaseHeaders[lowerName] = new HttpHeader(lowerName, value);
            }
        }
        public HttpHeader Get(string name, bool ignoreCase = false)
        {
            if(this._headers.TryGetValue(name, out HttpHeader value))
            {
                return value;
            }
            else
            {
                if(ignoreCase)
                {
                    if (this._lowcaseHeaders.TryGetValue(name.ToLower(), out HttpHeader lowerCaseValue))
                    {
                        return lowerCaseValue;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

    }
}

