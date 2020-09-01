using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json.Linq;

namespace PowerLine
{
    public class PowerLineWebsocketClient
    {

        public readonly HttpListenerContext FirstContext;
        public readonly Task mainTask;
        public readonly CancellationToken CancelToken;
        public readonly PowerLineServer Server;
        public HttpListenerWebSocketContext websocketContext;
        public WebSocket websocket;

        internal Dictionary<string, PowerLineEvent> events;
        internal object eventsLock;

        internal SemaphoreSlim sendLock;
       


        internal Dictionary<string, object> CustomValues;
        internal object CustomValueLock;

        public PowerLineWebsocketClient(PowerLineServer server, HttpListenerContext firstContext, CancellationToken cancelToken)
        {
            this.sendLock = new SemaphoreSlim(1, 1);
            this.CustomValueLock = new object();
            this.CustomValues = new Dictionary<string, object>();
            this.events = new Dictionary<string, PowerLineEvent>();
            this.eventsLock = new object();
            this.Server = server;
            this.FirstContext = firstContext;
            this.CancelToken = cancelToken;
            this.mainTask = new Task(InnerRunner, this.CancelToken, TaskCreationOptions.LongRunning);
            this.mainTask.Start();
        }
        public bool TryGetCustomValue(string valueName, out object value)
        {
            lock(this.CustomValueLock)
            {
                return this.CustomValues.TryGetValue(valueName, out value);
            }
        }
        public bool TryGetCustomValue<T>(string valueName, out T value)
        {
            if(this.TryGetCustomValue(valueName, out object rawValue))
            {
                value = (rawValue == null) ? default(T) : (T)rawValue;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }
        public bool UnsetCustomValue(string valueName)
        {
            lock(this.CustomValues)
            {
                return this.CustomValues.Remove(valueName);
            }
        }
        public bool IsCustomValueSet(string valueName)
        {
            lock(this.CustomValues)
            {
                return this.CustomValues.ContainsKey(valueName);
            }
        }
        public void SetCustomValue(string valueName, object value)
        {
            lock(this.CustomValueLock)
            {
                if(this.CustomValues.ContainsKey(valueName))
                {
                    this.CustomValues[valueName] = value;
                }
                else
                {
                    this.CustomValues.Add(valueName, value);
                }
            }
            
        }
        private async Task AcceptWebSocket()
        {
            this.websocketContext = await this.FirstContext.AcceptWebSocketAsync("json");
            this.websocket = this.websocketContext.WebSocket;
            Server.innerAddClient(this);
        }
        private async void InnerRunner()
        {
            try
            {
                await this.InnerRunnerMain();
            } 
            catch (Exception ex)
            {
                Console.WriteLine($"Websocket connection error: {ex.Message}");
                try
                {
                    CancellationTokenSource closeCancelToken = new CancellationTokenSource();
                    closeCancelToken.CancelAfter(new TimeSpan(0, 0, 20));
                    await this.websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed Connection", closeCancelToken.Token);
                }
                catch { }
            }
            finally
            {
                Server.innerRemoveClient(this);
            }
        }
        private async Task InnerRunnerMain()
        {
            await this.AcceptWebSocket();
            byte[] buffer = new byte[8192];
            while (!this.CancelToken.IsCancellationRequested)
            {
                MemoryStream data = new MemoryStream();
                bool dataFinished = false;
                while (!this.CancelToken.IsCancellationRequested && !dataFinished)
                {
                    WebSocketReceiveResult result = await this.websocket.ReceiveAsync(new ArraySegment<byte>(buffer), this.CancelToken);
                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                        case WebSocketMessageType.Binary:
                            data.Write(buffer, 0, result.Count);
                            break;
                        case WebSocketMessageType.Close:
                            return;
                    }
                    dataFinished = result.EndOfMessage;
                }
                data.Position = 0;
                await this.Server.HandleWebsocketMessageAsync(this, data);
            }
        }
        private void AddResponsePayload(JObject obj, PowerLineContext context)
        {
            if (context.ResponsePayload != null)
            {
                obj.Add("contentType", context.ResponseContentType.ToString());
                switch (context.ResponseContentType)
                {
                    case PowerLineContextContentType.Json:
                        JObject jsonPayload;
                        using (StreamReader reader = new StreamReader(context.ResponsePayload))
                        {
                            jsonPayload = JObject.Parse(reader.ReadToEnd());
                        }
                        obj.Add("payload", jsonPayload);
                        return;
                    case PowerLineContextContentType.Text:
                        string stringPayload;
                        using (StreamReader reader = new StreamReader(context.ResponsePayload))
                        {
                            stringPayload = reader.ReadToEnd();
                        }
                        obj.Add("payload", stringPayload);
                        return;
                    case PowerLineContextContentType.Unknown:
                        throw new Exception("Unkown content type can't send in websocket");
                }

            }
        }
        public async Task SendResponseAsync(PowerLineContext context, int websocketid)
        {
            JObject obj = new JObject();
            obj.Add("code", context.responseCode);
            obj.Add("websocketId", websocketid);
            if (context.responseText != null) obj.Add("text", context.responseText);
            if(context.ResponseHeader != null && context.ResponseHeader.Any())
            {
                obj.Add("headers", context.ResponseHeader.GetJson());
            }
            this.AddResponsePayload(obj, context);

            string rawJobject = obj.ToString(Newtonsoft.Json.Formatting.None);
            byte[] raw = System.Text.Encoding.UTF8.GetBytes(rawJobject);
            await this.SendRawData(raw);
        }
        internal void SubscribeEvent(PowerLineEvent eventObject)
        {
            lock(this.eventsLock)
            {
                this.events.Add(eventObject.Name, eventObject);
            }
        }
        internal bool UnsubscribeEvent(PowerLineEvent eventObject)
        {
            lock (this.eventsLock)
            {
                return this.events.Remove(eventObject.Name);
            }
        }
        internal async Task RaiseEventAsync(PowerLineEvent name, JToken message)
        {
            JObject obj = new JObject();
            obj.Add("code", 600);
            obj.Add("name", name.Name);
            obj.Add("payload", message);
            string rawJobject = obj.ToString(Newtonsoft.Json.Formatting.None);
            byte[] raw = System.Text.Encoding.UTF8.GetBytes(rawJobject);
            await this.SendRawData(raw);
        }

        internal async Task SendRawData(byte[] raw)
        {
            await this.sendLock.WaitAsync();
            try
            {
                await this.websocket.SendAsync(new ArraySegment<byte>(raw), WebSocketMessageType.Text, true, this.CancelToken);
            }
            finally
            {
                this.sendLock.Release();
            }
            
        }
        public async Task<bool> RaiseEventAsync(string eventName, JObject message)
        {
            PowerLineEvent eventObject;
            lock (this.eventsLock)
            {
                if(!this.events.TryGetValue(eventName, out eventObject))
                {
                    return false;
                }
            }
            await this.RaiseEventAsync(eventObject, message);
            return true;
        }
    }
}
