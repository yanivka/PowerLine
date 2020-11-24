using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using PowerLine.Upnp;
using PowerLine.Upnp.UpnpControl;
using PowerLine.Upnp.UpnpCustomPackets;
using PowerLine.Upnp.UpnpStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PowerLine
{


    public class PowerLineServer:IDisposable
    {

        private HttpListener mainListener;
        private CancellationTokenSource cancelTokenSource;
        private CancellationToken cancelToken;
        private Task serverTask;
        private EventWaitHandle serverStopped;

        private object endpointsLock;
        private Dictionary<string, PowerLineEndPoint> endPoints;

        private List<PowerLineWebsocketClient> websocketClients;
        private object websocketClientLock;
        private Dictionary<string, PowerLineEvent> websocketEvents;
        private object websocketEventLock;

        public event EventHandler<PowerLineServer> OnStart;
        public event EventHandler<PowerLineServer> OnStop;
        public event EventHandler<HttpListenerContext> OnWebsocketClient;
        public event EventHandler<HttpListenerContext> OnHttpClient;
        public event EventHandler<Exception> OnError;

        private Upnp.UpnpEngine upnpEngine;
        private PowerLineEndPoint upnpEndpoint;

        public readonly IPAddress BindAddress;
        public readonly int BindPort;

        public PowerLineServer(IPAddress bindAddress, int bindPort, bool allowEvents = false, IEnumerable<PowerLineEndPoint> endpoints = null)
        {
            this.BindAddress = bindAddress;
            this.BindPort = bindPort;
            this.endpointsLock = new object();
            this.upnpEngine = new Upnp.UpnpEngine(this);
            this.upnpEndpoint = Upnp.PowerLineUpnpHttpHandle.GetEndpoint(this.upnpEngine);
            this.websocketEventLock = new object();
            this.websocketEvents = new Dictionary<string, PowerLineEvent>();
            this.websocketClientLock = new object();
            this.websocketClients = new List<PowerLineWebsocketClient>();
            this.serverStopped = new EventWaitHandle(true, EventResetMode.ManualReset);

            this.mainListener = new HttpListener();
            this.mainListener.Prefixes.Add(this.BuildBindUrl());

            this.endPoints = (endpoints == null) ? new Dictionary<string, PowerLineEndPoint>() : new Dictionary<string, PowerLineEndPoint>(endpoints.Select((item) => new KeyValuePair<string, PowerLineEndPoint>(item.EndPointName, item)));
            if (allowEvents)
            {
                this.AddEndpoint(PowerLineEventHandler.GetEndPoint(this));
            }
        }


        internal string BuildBindUrl() => (this.BindAddress == IPAddress.Any) ? BuildBindUrl("*", this.BindPort) : BuildBindUrl(this.BindAddress.ToString(), this.BindPort);
        public static string BuildBindUrl(string address, int port) => $"http://{address}:{port}/";
        public PowerLineEndPoint AddEndpoint(PowerLineEndPoint endpoint)
        {
            lock(this.endpointsLock)
            {
                if (this.endPoints.ContainsKey(endpoint.EndPointName))
                {
                    this.endPoints[endpoint.EndPointName] = endpoint;
                }
                else
                {
                    this.endPoints.Add(endpoint.EndPointName, endpoint);
                }
                return endpoint;
            }
            
        }
        public PowerLineEndPoint RemoveEndpoint(PowerLineEndPoint endpoint)
        {
            lock (this.endpointsLock)
            {
                this.endPoints.Remove(endpoint.EndPointName);
                return endpoint;
            }
        }
        public PowerLineEvent CreateEvent(string eventName )
        {
            PowerLineEvent eventObject = new PowerLineEvent(eventName);
            return this.AddEvent(eventObject);
        }
        public PowerLineEvent AddEvent(PowerLineEvent powerLineEvent)
        {
            lock(this.websocketEventLock)
            {
                this.websocketEvents.Add(powerLineEvent.Name, powerLineEvent);
            }
            return powerLineEvent;
        }
        public void Start()
        {
            this.Stop();
            
            this.mainListener.Start();
            this.cancelTokenSource = new CancellationTokenSource();
            this.cancelToken = this.cancelTokenSource.Token;
            this.serverTask = new Task(this.ServerLoop, this.cancelToken, TaskCreationOptions.LongRunning);
            this.serverStopped.Reset();
            this.serverTask.Start();

        }
        public void Stop()
        {
            this.cancelTokenSource?.Cancel();
            this.mainListener.Stop();
            this.serverTask?.Wait();
        }
        public void Wait()
        {
            this.serverStopped?.WaitOne();
        }
        public bool Wait(TimeSpan timeout)
        {
            if (this.serverStopped == null) return true;
            return this.serverStopped.WaitOne(timeout);
        }
        public void Dispose()
        {
            this.Stop();
        }

        public void StartUpnp()
        {
            this.AddEndpoint(this.upnpEndpoint);
            this.upnpEngine.Start();



            UpnpRootDevice device = new UpnpRootDevice(
                Guid.NewGuid(),
                "LampForMenahem",
                new UpnpNtDeviceType("Eva3Light", "1"),
                new UpnpManufacturer("fotonica"), 
                new UpnpModel("Eva3"));

            device.PresentationURL = "http://192.168.0.250";

            this.upnpEngine.AddDevice(device);
            Console.WriteLine($"Running for: {device.Id.ToString()}");
        }
        public void StopUpnp()
        {
            this.upnpEngine.Stop();
            this.RemoveEndpoint(this.upnpEndpoint);
        }

        public PowerLineWebsocketClient[] GetAllClients()
        {
            lock(this.websocketClientLock)
            {
                return this.websocketClients.ToArray();
            }
        }
        private async Task<PowerLineEndPointExecutionResult> GetHandleResultAsync(HttpListenerContext context)
        {
            string[] UrlPath = context.Request.Url.AbsolutePath.Split('/');
            PowerLineContext powerlineContext = new PowerLineContext(context, 0, UrlPath, context.Request.Url);
            return await GetHandleResultAsync(powerlineContext);
        }
        private PowerLineEndPoint GetDynamicEndpoint(Uri url)
        {
            lock (this.endpointsLock)
            {
                foreach (KeyValuePair<string, PowerLineEndPoint> endpoint in this.endPoints)
                {
                    if (endpoint.Value.Dynamic && endpoint.Value.VerifyDynamicEndpoint(url)) return endpoint.Value;
                }
                return null;
            }
        }
        private bool GetEndPoint(string name, out PowerLineEndPoint endpoint)
        {
            lock (this.endpointsLock)
            {
                if (this.endPoints.TryGetValue(name, out endpoint))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        private async Task<PowerLineEndPointExecutionResult> GetHandleResultAsync(PowerLineContext context)
        {
            context.ResponseHeader["Server"] = "PowerLine powered by ";
            if (this.GetEndPoint(context.Path[1], out PowerLineEndPoint endpoint))
            {
                return await endpoint.OnRequestAsync(2, context.Path, context);
            }
            else
            {
                PowerLineEndPoint dyanmicEndPoint = this.GetDynamicEndpoint(context.ReqeustUri);
                if (dyanmicEndPoint != null)
                {
                    return await dyanmicEndPoint.OnRequestAsync(2, context.Path, context);
                }
                return new PowerLineEndPointExecutionResult(context, PowerLinExecutionResultType.EndPointNotFound, null);
            }
        }
        internal async Task HandleWebsocketMessageAsync(PowerLineWebsocketClient client, Stream message)
        {
            JObject mainMessage;
            using (StreamReader reader = new StreamReader(message))
            {
                mainMessage = JObject.Parse(reader.ReadToEnd());
            }

            if (!mainMessage.TryGetValue("url", out string requestUrl))
            {
                throw new Exception("Invaild request url");
            }
            if (!mainMessage.TryGetValue("websocketId", out int websocketId))
            {
                throw new Exception("Invaild request url");
            }
            string[] UrlPath = requestUrl.Split('/');
            PowerLineContext context = new PowerLineContext(mainMessage, message, 0, UrlPath, client);

            PowerLineEndPointExecutionResult result = await GetHandleResultAsync(context);
            switch (result.ResultType)
            {
                case PowerLinExecutionResultType.EndPointNotFound:
                    result.Context.SetResponse(404);
                    result.Context.SetResponseHttpString("Not Found");
                    break;
                case PowerLinExecutionResultType.HandlerException:
                    result.Context.SetResponse(500);
                    result.Context.SetResponseHttpString(result.Exception.Message);
                    break;
                case PowerLinExecutionResultType.HttpMethodNotFound:
                    result.Context.SetResponse(404);
                    result.Context.SetResponseHttpString("Not Found [HttpMethod]");
                    break;
            }

            await client.SendResponseAsync(context, websocketId);
        }
        private async Task HandleContextAsync(HttpListenerContext context)
        {
            PowerLineEndPointExecutionResult result = await GetHandleResultAsync(context);
            switch(result.ResultType)
            {
                case PowerLinExecutionResultType.EndPointNotFound:
                    result.Context.SetResponse(404);
                    result.Context.SetResponseHttpString("Not Found");
                    break;
                case PowerLinExecutionResultType.HandlerException:
                    result.Context.SetResponse(500);
                    result.Context.SetResponseHttpString(result.Exception.Message);
                    break;
                case PowerLinExecutionResultType.HttpMethodNotFound:
                    result.Context.SetResponse(404);
                    result.Context.SetResponseHttpString("Not Found [HttpMethod]");
                    break;
            }
            result.Context.response.StatusCode = result.Context.responseCode;
            if (result.Context.responseText != null) result.Context.response.StatusDescription = result.Context.responseText;
            result.Context.response.Headers.Clear();
            if(result.Context.ResponseHeader != null)
            {
                foreach (KeyValuePair<string, string> header in result.Context.ResponseHeader)
                {
                    result.Context.response.Headers.Add(header.Key, header.Value);
                }
            }
            if (result.Context.ResponsePayloadLength != -1) result.Context.response.ContentLength64 = result.Context.ResponsePayloadLength;
            if (result.Context.ResponsePayload != null)
            {
                result.Context.ResponsePayload.CopyTo(result.Context.response.OutputStream);
                result.Context.ResponsePayload.Close();
            }
        
            result.Context.response.Close(); // Send the response to the remote endpoint
        } 
        private void handleAsyncContext(HttpListenerContext context)
        {
            Task.Run(async () => await this.HandleContextAsync(context));
        }
        private void handleAsyncWebsocket(HttpListenerContext context)
        {
            // just create a client, he will manage eveyrthing else (like adding to the server client list)
            PowerLineWebsocketClient client = new PowerLineWebsocketClient(this, context, this.cancelToken);
        }
        private async void ServerLoop()
        {
            try
            {
                this.OnStart?.Invoke(this, this);
                while (!this.cancelToken.IsCancellationRequested)
                {
                    HttpListenerContext context = await this.mainListener.GetContextAsync();
                    if(context.Request.IsWebSocketRequest)
                    {
                        this.OnWebsocketClient?.Invoke(this, context);
                        handleAsyncWebsocket(context);
                    }
                    else
                    {
                        this.OnHttpClient?.Invoke(this, context);
                        handleAsyncContext(context);
                    }
                }
            }
            finally
            {
                this.cancelTokenSource.Cancel();
                this.serverStopped.Reset();
                this.OnStop?.Invoke(this, this);
            }                     
        }

        internal void innerUnsbscribeAllEvents(PowerLineWebsocketClient client)
        {
            lock(client.eventsLock)
            {
                foreach(KeyValuePair<string, PowerLineEvent> singleEvent in client.events)
                {
                    lock (singleEvent.Value.clientsLock)
                    {
                        singleEvent.Value.clients.Remove(client);
                    }
                }
                client.events.Clear();
            }
        }
        internal bool innerSubscribeEvent(PowerLineWebsocketClient client, PowerLineEvent powerLineEvent)
        {
            if(powerLineEvent.SubscribeWithCheck(client))
            {
                client.SubscribeEvent(powerLineEvent);
                return true;
            }
            else
            {
                return false;
            }
        }
        public PowerLineEvent GetEvent(string eventName)
        {
            lock(this.websocketEventLock)
            {
                if(this.websocketEvents.TryGetValue(eventName, out PowerLineEvent singleEvent))
                {
                    return singleEvent;
                }
                else
                {
                    return null;
                }
            }
        }
        internal void innerUnsbscribeEvent(PowerLineWebsocketClient client, PowerLineEvent powerLineEvent)
        {
            client.UnsubscribeEvent(powerLineEvent);
            powerLineEvent.UnsubscribeClient(client);
        }


        internal void innerAddClient(PowerLineWebsocketClient client)
        {
            lock(this.websocketClientLock)
            {
                this.websocketClients.Add(client);
            }
        }
        internal bool innerRemoveClient(PowerLineWebsocketClient client)
        {
            this.innerUnsbscribeAllEvents(client);
            lock (this.websocketClientLock)
            {
                return this.websocketClients.Remove(client);
            }
        }
    }
}
