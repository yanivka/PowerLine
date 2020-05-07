using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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


        public PowerLineWebsocketClient(PowerLineServer server, HttpListenerContext firstContext, CancellationToken cancelToken)
        {
            this.Server = server;
            this.FirstContext = firstContext;
            this.CancelToken = cancelToken;
            this.mainTask = new Task(InnerRunner, this.CancelToken, TaskCreationOptions.LongRunning);
            this.mainTask.Start();
        }


        private async Task AcceptWebSocket()
        {
            this.websocketContext = await this.FirstContext.AcceptWebSocketAsync(null);
            this.websocket = this.websocketContext.WebSocket;
            Server.innerAddClient(this);
        }
        private async void InnerRunner()
        {
            await this.AcceptWebSocket();
            byte[] buffer = new byte[2048];
            while(!this.CancelToken.IsCancellationRequested)
            {
                WebSocketReceiveResult result = await this.websocket.ReceiveAsync(new ArraySegment<byte>(buffer), this.CancelToken);
                string message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"{result.MessageType}");
                Console.WriteLine($"Websocket: {message}");
            }
        }
    }
}
