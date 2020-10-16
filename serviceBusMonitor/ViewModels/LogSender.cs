using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace serviceBusMonitor.ViewModels
{
    class LogSender
    {
        #region private property
        private WebSocket ws = null;
        private readonly CancellationTokenSource tokenSource;
        #endregion


        #region public property
        public event EventHandler OnStateChanged;
        public bool IsConnected { get; private set; } = false;

        public EventHandler<MessageEventArgs> SetOnMessage
        {
            set
            {
                ws.OnMessage += value;
            }
        }
        #endregion


        public void SetUrlOrPort(string urlOrPort)
        {
            Task.Run(() =>
            {
                try
                {
                    int port = 0;
                    var isNumeric = int.TryParse(urlOrPort, out port);
                    ws = (isNumeric)?
                        new WebSocket($"ws://localhost:{port}"):
                        new WebSocket(urlOrPort);
                    ws.OnOpen += Ws_OnOpen;                    
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            var jt = JToken.Parse(e.Data);
            switch (jt.Type)
            {
                case JTokenType.Object:
                {
                        var x = jt["msg"];
                        if (x.ToString().ToLower() == "ok") IsConnected=true;
                }break;
            }
        }

        #region public methods
        public bool StartLogging()
        {
            if (!IsConnected) {
                ws.Connect();
                return IsConnected;
            }
            else
            {
                ws.Close();
                return IsConnected;
            }
        }
        #endregion


        #region private methods
        private void Ws_OnOpen(object sender, EventArgs e)
        {
            ws.Send(JsonConvert.SerializeObject(new { type = "connecting", msg = "start" }));
        }
        #endregion
    }
}
