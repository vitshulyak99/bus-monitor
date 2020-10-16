using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using serviceBusMonitor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using ConsoleApp5;
namespace serviceBusMonitor.ViewModels
{

    class Streaming
    {
        static public readonly DateTime dateTime = new DateTime(1970, 1, 1, 2, 59, 59);
        public readonly static string Stage = "#";
        public readonly static string Prod = "#";
        static public readonly string PH = "PricesHub";
        static public readonly string ONP = "OnNewPrice";
        static public readonly string STPS = "SubscribeToPriceStream";
        static public readonly string UFPS = "SubscribeFromPriceStream";
        static public readonly string STOBS = "SubscribeToOrderBookStream";
        static public readonly string UFOBS = "UnsubscribeFromOrderBookStream";
        static public readonly string ONOB = "OnNewOrderBook";
        static public readonly string OH = "OrderBookHub";

        static public Brush setbg(double delay, string ts = "")
        {
            if (ts != "N") return Brushes.OrangeRed;
            if (delay == 0) return Brushes.White;
            if (delay > 5 && delay < 15) return Brushes.LightGreen;
            if (delay >= 15 && delay < 40) return Brushes.Yellow;
            if (delay >= 40) return Brushes.DarkRed;
            return Brushes.White;

        }

        static public Brush setfg(double delay)
        {
            if (delay >= 40) return Brushes.White;
            return Brushes.Black;
        }

        SignalRClient client;

        public DateTime LastUpdate { get; private set; }
        Dictionary<string, LPandOB> Values;

        object locker = new object();

        private bool IsRun = false;
        bool IsProduction = false;

        public IEnumerable<LPandOB> Collection
        {
            get
            {
                lock (locker)
                    return Values.Values;
            }
        }

        public Streaming(string Url)
        {
            var sn = SetShNFromConfig().ToList();
            sn.Sort((x, y) => x.CompareTo(y));
            Values = new Dictionary<string, LPandOB>();

            foreach (var s in sn)
            {
                Values.Add(s, new LPandOB { ShortName = s });
            }

            client = new SignalRClient(new HubConnection(Url), sn.ToArray());
            client.SetOrderBookEventMethod(UpdateOB);
            client.SetPriceEventMethod(UpdateLP);

        }
        class Config
        {
            public string BaseCurrency { get; set; }
            public List<string> InstrumentCurrencys { get; set; }
        }

        private IEnumerable<string> SetShNFromConfig()
        {

            var sr = new StreamReader("JSON_CONFIGS/shortnames.json");
            var alltext = sr.ReadToEnd();
            var config = JsonConvert.DeserializeObject<Config[]>(alltext);
            List<string> res = new List<string>(config.Length);

            foreach (var item1 in config)
            {
                foreach (var item2 in item1.InstrumentCurrencys)
                {
                    res.Add(item1.BaseCurrency + "/" + item2);
                }
            }

            return res;
        }

        public void Stop()
        {
            if (IsRun)
            {
                client.Stop();
                IsRun = false;
            }
        }

        public void Run()
        {
            if (!IsRun)
            {
                client.Run().Wait();
                client.Subscribe();
                IsRun = true;
            }
        }

        public string GetStreamingStatus()
        {
            return !IsRun ? "Stoped" : "Running";
        }

        private void UpdateOB(dynamic orderbook)
        {
            OrderBook ob = new OrderBook { ShortName = orderbook.shortName, Date = Streaming.dateTime.AddSeconds((double)orderbook.orderBookDateTime) };

            lock (locker)
                Values[ob.ShortName].UpdateValueOB(ob);

            LastUpdate = DateTime.Now;
        }

        private void UpdateLP(dynamic lastprice)
        {
            LastPriceNew lp = JsonConvert.DeserializeObject<LastPriceNew>($"{lastprice}");

            lock (locker)
                Values[lp.ShortName].UpdateValueLP(lp);

            LastUpdate = DateTime.Now;
        }
    }
}
