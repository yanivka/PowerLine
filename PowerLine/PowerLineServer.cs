using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.WebSockets;
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

        private Dictionary<string, PowerLineEndPoint> endPoints;

        private List<PowerLineWebsocketClient> websocketClients;
        private object websocketClientLock;

        public event EventHandler<PowerLineServer> OnStart;
        public event EventHandler<PowerLineServer> OnStop;
        public event EventHandler<Exception> OnError;

      
        public PowerLineServer(string bindDomain, IEnumerable<PowerLineEndPoint> endpoints = null)
        {
            this.websocketClientLock = new object();
            this.websocketClients = new List<PowerLineWebsocketClient>();
            this.serverStopped = new EventWaitHandle(true, EventResetMode.ManualReset);

            this.mainListener = new HttpListener();
            this.mainListener.Prefixes.Add(bindDomain);

            this.endPoints = (endpoints == null) ? new Dictionary<string, PowerLineEndPoint>() : new Dictionary<string, PowerLineEndPoint>(endpoints.Select((item) => new KeyValuePair<string, PowerLineEndPoint>(item.EndPointName, item)));
        }
               
        public PowerLineEndPoint AddEndpoint(PowerLineEndPoint endpoint)
        {
            this.endPoints.Add(endpoint.EndPointName, endpoint);
            return endpoint;
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

        private PowerLineEndPointExecutionResult GetHandleResult(HttpListenerContext context)
        {
            string[] UrlPath = context.Request.Url.AbsolutePath.Split('/');
            PowerLineContext powerlineContext = new PowerLineContext(context, 0, UrlPath);
            context.Response.Headers["Server"] = "PowerLine powered by ";

            if (this.endPoints.TryGetValue(UrlPath[1], out PowerLineEndPoint endpoint))
            {
                return endpoint.OnRequest(1, UrlPath, powerlineContext);
            }
            else
            {
                return new PowerLineEndPointExecutionResult(powerlineContext, PowerLinExecutionResultType.EndPointNotFound, null);
            }
        }
        private void HandleContext(HttpListenerContext context)
        {
            PowerLineEndPointExecutionResult result = GetHandleResult(context);
            switch(result.ResultType)
            {
                case PowerLinExecutionResultType.EndPointNotFound:
                    break;
                case PowerLinExecutionResultType.HandlerException:
                    break;
                case PowerLinExecutionResultType.HttpMethodNotFound:
                    break;
                case PowerLinExecutionResultType.OK:
                    break;
            }
            result.Context.response.Close(); // Send the response to the remote endpoint
        } 
        private void handleAsyncContext(HttpListenerContext context)
        {
            Task.Run(() => this.HandleContext(context));
        }
        private void handleAsyncWebsocket(HttpListenerContext context)
        {
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
                    Console.WriteLine($"Got message {context.Request.Url.AbsolutePath}");
                    if(context.Request.IsWebSocketRequest)
                    {
                        Console.WriteLine("got websocket!!");
                        handleAsyncWebsocket(context);
                    }
                    else
                    {
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

        internal void innerAddClient(PowerLineWebsocketClient client)
        {
            lock(this.websocketClientLock)
            {
                this.websocketClients.Add(client);
            }
        }
        internal bool innerRemoteClient(PowerLineWebsocketClient client)
        {
            lock (this.websocketClientLock)
            {
                return this.websocketClients.Remove(client);
            }
        }
    }
}
