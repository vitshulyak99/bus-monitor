using serviceBusMonitor.ViewModels;
using System;
using System.Windows.Media;

namespace serviceBusMonitor.Models
{
    class LPandOB
    {
        public LPandOB( )
        {
            DelayLP = 0;
            DelayOB = 0;
            LastUpdatePriceDate = DateTime.Now;
            DateTimeOB = DateTime.Now;
            DateTimeLP = DateTime.Now;
            LastPrice = 1;
        }

        public string ShortName { get; set; }

    
        public double DelayLP { get; private set; }
        public double DelayOB { get; private set; } 
        public DateTime LastUpdatePriceDate{ get; private set; }

        public DateTime DateTimeOB { get;  set; }
        public double Bid { get; private set; }
        public double Ask { get; private set; }
        public DateTime DateTimeLP { get;  set; }

        public Brush Foreground
        {
            get
            {
                return DelayOB > DelayLP ? Streaming.setfg(DelayOB) : Streaming.setfg(DelayLP);
            }
        }


        public Brush Background
        {
            get
            {
                if (IsOBJumping)
                {
                    IsOBJumping = false;
                    return Brushes.Aquamarine;
                }

                var lpupd = (DateTime.Now - LastUpdatePriceDate).TotalSeconds;

                if (lpupd > 60)
                {
                    return Brushes.Blue;
                }

                return DelayOB > DelayLP ? Streaming.setbg(DelayOB, TradingStatus) : Streaming.setbg(DelayLP, TradingStatus);
            }
        }

        public double LastPrice { get; set; }

        public string TradingStatus { get; set; }
        public bool IsOBJumping { get; private set; }

        public void SetDelays()
        {
            DelayLP = Math.Round( (DateTime.Now - DateTimeLP).TotalSeconds,3);
            DelayOB = Math.Round((DateTime.Now - DateTimeOB).TotalSeconds,3);
        }

        internal void UpdateValueOB(OrderBook ob)
        {
            this.DateTimeOB = DateTime.Now;
        }

        internal void UpdateValueLP(LastPriceNew lp)
        {
            if(this.LastPrice / lp.Price > 0)
            {
                this.LastUpdatePriceDate = DateTime.Now;
                this.LastPrice = lp.Price;
            }

            if(Bid / lp.bid > 0.005)
            {
                IsOBJumping = true;
            }

            this.Bid = lp.bid;
            this.Ask = lp.ask;
            
            this.DateTimeLP = DateTime.Now;
            this.TradingStatus = lp.TradingStatus;
        }
    }
}
