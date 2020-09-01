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
        Json = 2
    }
    public class PowerLineContext
    {
        public readonly HttpListenerRequest request;
        public readonly HttpListenerResponse response;
        public readonly HttpListenerContext context;

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
        internal PowerLineContextContentType ResponseContentType = PowerLineContextContentType.Unknown;

        public readonly string RequestMethod;

        internal int responseCode = 404;
        internal string responseText = null;


        private JObject _requestJsonPayload;
        

        public PowerLineContext(JObject message,Stream requestPayload,int pathIndex, string[] path, PowerLineWebsocketClient client)
        {
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
        public void SetResponse(int httpCode)
        {
            this.responseCode = httpCode;
        }
        public void SetResponse(int httpCode, byte[] payload)
        {
            this.ResponsePayloadLength = payload.Length;
            this.SetResponse(httpCode, new MemoryStream(payload));
        }
        public void SetResponse(int httpCode, string payload) {
            this.ResponseContentType = PowerLineContextContentType.Text;
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
