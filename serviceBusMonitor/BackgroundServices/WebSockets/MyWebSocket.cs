using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fleck;
using Newtonsoft.Json;


namespace serviceBusMonitor.BackgroundServices.WebSockets
{
    class MyWebSocket
    {
        WebSocketServer server = null;
        List<IWebSocketConnection> clients;

        public MyWebSocket(string url)
        {
            server = new WebSocketServer(url);
            clients = new List<IWebSocketConnection>();
        }

        public void Start()
        {
            server.Start(startServer);
        }

        public void Stop()
        {
            server.Dispose();
        }

        void startServer(IWebSocketConnection socket)
        {
            socket.OnOpen += () => { clients.Add(socket); };
            socket.OnClose += () => { clients.Remove(socket); };
            socket.OnMessage += (msg) => { };
            socket.OnError += (e) => { };
        }

        public void SendLogToUser(string t, object m)
        {
            Task.Run(async () =>
            {
                var message = JsonConvert.SerializeObject(new { @type = t, msg = m });
                await Task.Run(() => clients.ToList().ForEach((c) => { c.Send(message); }));
            });
        }





    }
}
