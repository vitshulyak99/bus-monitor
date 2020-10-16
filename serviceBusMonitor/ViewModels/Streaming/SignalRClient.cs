using Microsoft.AspNet.SignalR.Client;
using serviceBusMonitor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp5
{
    public enum TypeSubscribe
    {
        Price,
        OrderBook
    }

    public delegate void MyEventHandler(dynamic data);
    public class SignalRClient
    {
        readonly HubConnection _hub = null;
        IHubProxy _proxyOrderBook = null;
        IHubProxy _proxyPrice = null;

        private static string SubscribeOrderBook = "SubscribeToOrderBookStream";
        private static string EventOrderBook = "OnNewOrderBook";
        private static string EventPrice = "OnNewPrice";
        private static string SubscribePrice = "SubscribeToPriceStream";
        private static string UnsubscribeFrom = "UnsubscribeFrom";

        bool _sPrice = false;
        bool _sOrder = false;
        string[] _args;
        TypeSubscribe Type;

        public HubConnection Hub => _hub;

        public SignalRClient(HubConnection hub, string[] args)
        {

            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
            _args = args ?? throw new ArgumentNullException(nameof(args));

            _proxyPrice = _hub.CreateHubProxy("PricesHub");
            _proxyPrice.On(EventPrice, (x) => OnNewPrice(x));
            _proxyOrderBook = _hub.CreateHubProxy("OrderBookHub");
            _proxyOrderBook.On(EventOrderBook, (x) => OnNewOrderBook(x));
        }

        event MyEventHandler OnNewPrice;
        event MyEventHandler OnNewOrderBook;

        public void SetOrderBookEventMethod(MyEventHandler myEvent)
        {
            OnNewOrderBook += myEvent;
        }
        public void SetPriceEventMethod(MyEventHandler myEvent)
        {
            OnNewPrice += myEvent;
        }

        public async Task Run()
        {
            await _hub.Start();
        }

        public void Stop()
        {
            _hub.Stop();
        }

        public void Subscribe()
        {
            SubscribeToPrice();
            SubscribeToOrderBook();
        }

        public void Unsubscribe()
        {
            UnsubscribeFromPrice();
            UnsubscribeFromOrderBook();
        }

        private void SubscribeToPrice()
        {
            if (_hub.State == ConnectionState.Connected)
                Parallel.ForEach(_args, x => _proxyPrice.Invoke("SubscribeToPriceStream", x));
            else throw new Exception("No server conection");
        }

        private void SubscribeToOrderBook()
        {
            if (_hub.State == ConnectionState.Connected)
                Parallel.ForEach(_args, (x) => _proxyOrderBook.Invoke(SubscribeOrderBook, x));
            else throw new Exception("No server conection");
        }

        private void UnsubscribeFromOrderBook()
        {
            if (_sOrder)
                if (_hub.State == ConnectionState.Connected)
                    Parallel.ForEach(_args, (x) => _proxyOrderBook.Invoke(UnsubscribeFrom + "OrderBookStream", x));
                else throw new Exception("No server conection");
        }

        private void UnsubscribeFromPrice()
        {
            if (_sPrice)
                if (_hub.State == ConnectionState.Connected)
                    Parallel.ForEach(_args, x => _proxyPrice.Invoke(UnsubscribeFrom + "PriceStream", x));
                else throw new Exception("No server conection");
        }
    }
}
