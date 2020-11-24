using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace PowerLine
{
    public enum PowerLineContextContentType
    {
        Unknown = 0,
        Text = 1,
        Json = 2,
        Html = 3,
        Xml = 4
    }
    public static class PowerLineContextContentTypeExtentions
    {
        public static string GetMimeType(this PowerLineContextContentType contextType)
        {
            switch(contextType)
            {
                case PowerLineContextContentType.Json:
                    return "application/json";
                case PowerLineContextContentType.Text:
                    return "text/plain";
                case PowerLineContextContentType.Html:
                    return "text/plain";
                case PowerLineContextContentType.Xml:
                    return "application/xml";
                default:
                    return null;
            }
        }
    }
    public class PowerLineContext
    {
        public readonly HttpListenerRequest request;
        public readonly HttpListenerResponse response;
        public readonly HttpListenerContext context;

        public readonly string RemoteAddress;
        public readonly string LocalAddress;
        public readonly int RemotePort;
        public readonly int LocalPort;

        public int PathIndex;
        public readonly string[] Path;
        public readonly Uri ReqeustUri;
        public readonly Dictionary<string, string> RequestHeaders;
        public readonly Dictionary<string, string> ResponseHeader;
        public readonly bool IsWebSocket;
        public readonly PowerLineWebsocketClient WebsocketClient;
        public readonly PowerLineUriArgumentParser RequestedUriQuary;

        public Stream RequestPayload;
        public int ResponsePayloadLength = -1;
        public Stream ResponsePayload;

        private PowerLineContextContentType _responseContentType = PowerLineContextContentType.Unknown;
        internal PowerLineContextContentType ResponseContentType { 
            get
            {
                return this._responseContentType;
            }
            set
            {
                this._responseContentType = value;
                if (this._responseContentType != PowerLineContextContentType.Unknown) this.SetRepsonseContentType(this._responseContentType.GetMimeType());
            }
        } 

        public readonly string RequestMethod;

        internal int responseCode = 404;
        internal string responseText = null;


        private JObject _requestJsonPayload;
        

        public PowerLineContext(JObject message,Stream requestPayload,int pathIndex, string[] path, PowerLineWebsocketClient client)
        {
            this.RemoteAddress = client.RemoteAddress;
            this.RemotePort = client.RemotePort;
            this.LocalPort = client.LocalPort;
            this.RemoteAddress = client.RemoteAddress;

            this.WebsocketClient = client;
            this.IsWebSocket = true;
            this.context = null;
            this.request = null;
            this.response = null;
            this.PathIndex = pathIndex;
            this.Path = path;
            this._requestJsonPayload = message;

            if(message.TryGetValue("method", out string requestMethod))
            {
                this.RequestMethod = requestMethod;
            }
            else
            {
                throw new Exception("Invaild websocket request");
            }

            this.RequestHeaders = message.ReadHeaders();
            this.ResponseHeader = new Dictionary<string, string>();
            this.RequestPayload = requestPayload;
            this.ResponsePayload = null;
        }
        public PowerLineContext(HttpListenerContext context, int pathIndex, string[] path, Uri requestUrl)
        {
            this.LocalAddress = context.Request.LocalEndPoint.Address.ToString();
            this.LocalPort = context.Request.LocalEndPoint.Port;
            this.RemoteAddress = context.Request.RemoteEndPoint.Address.ToString();
            this.RemotePort = context.Request.RemoteEndPoint.Port;

            this.ReqeustUri = requestUrl;
            this.RequestedUriQuary = new PowerLineUriArgumentParser(this.ReqeustUri);
            this.IsWebSocket = false;
            this.context = context;
            this.request = context.Request;
            this.response = context.Response;
            this.RequestMethod = this.request.HttpMethod;
            this.PathIndex = pathIndex;
            this.Path = path;
            this.RequestHeaders = new Dictionary<string, string>(this.request.Headers.AllKeys.Select((headerKey) => new KeyValuePair<string, string>(headerKey, this.request.Headers.Get(headerKey))));
            this.ResponseHeader = new Dictionary<string, string>(this.request.Headers.AllKeys.Select((headerKey) => new KeyValuePair<string, string>(headerKey, this.request.Headers.Get(headerKey))));
            this.RequestPayload = (this.request.HasEntityBody) ? this.request.InputStream : null;
            this.ResponsePayload = null;
        }

        public async Task<JObject> ReadResponsePayloadAsJson()
        {
            if(this._requestJsonPayload == null && this.RequestPayload != null)
            {
                this._requestJsonPayload = JObject.Parse(await this.RequestPayload.ReadToEndStringAsync());
            }
            return this._requestJsonPayload;
        }
        public void SetResponseHttpString(string httpString)
        {
            this.responseText = httpString;
        }
        public void SetRepsonseContentType(string mimeType)
        {

            if (this.ResponseHeader.ContainsKey("Content-Type"))
            {
                this.ResponseHeader["Content-Type"] = mimeType;
            }
            else
            {
                this.ResponseHeader.Add("Content-Type", mimeType);
            }
        }
        public void SetResponse(int httpCode)
        {
            this.responseCode = httpCode;
        }
        public void SetResponse(int httpCode, byte[] payload)
        {
            this.ResponsePayloadLength = payload.Length;
            this.SetResponse(httpCode, new MemoryStream(payload));
        }
        public void SetResponse(int httpCode, string payload, bool setTextMime = true) {
            if(setTextMime) this.ResponseContentType = PowerLineContextContentType.Text;
            this.SetResponse(httpCode, System.Text.Encoding.UTF8.GetBytes(payload)); 
        }
        public void SetResponse(int httpCode, JObject obj)
        {
            this.ResponseContentType = PowerLineContextContentType.Json;
            this.SetResponse(httpCode, System.Text.Encoding.UTF8.GetBytes(obj.ToString(Newtonsoft.Json.Formatting.None)));
        }
        public void SetResponse(int httpCode, JObject obj, Newtonsoft.Json.Formatting formatting)
        {
            this.ResponseContentType = PowerLineContextContentType.Json;
            this.SetResponse(httpCode, System.Text.Encoding.UTF8.GetBytes(obj.ToString(formatting)));
        }
        public void SetResponse(int httpCode, Stream payload)
        {
            
            this.SetResponse(httpCode);
            this.ResponsePayload = payload;
        }
    }
}
