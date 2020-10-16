using serviceBusMonitor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace serviceBusMonitor.Models
{

    public class LastPrice : INotifyPropertyChanged
    {
        
    
        private string shortName;
        private double delay;
        private int date;
        private string tradingStatus;
        public string TradingStatus { get=>tradingStatus; set=>tradingStatus =value; }

        public string ShortName
        {
            get
            {
                return shortName;
            }
            set
            {
                shortName = value;

            }
        }
        public int Date
        {
            get
            {
                return date;
            }
            set
            {
                Delay = Math.Round((DateTime.Now -Streaming.dateTime.Add(TimeSpan.FromSeconds(date))).TotalSeconds, 4);
                date = value;
                // OnPropertyChanged("Date");
            }
        }
        public double Price { get; set; }
        public double Delay
        {
            get { return delay; }

            set
            {
                delay = value;    
                BG = Streaming.setbg(delay);
                if ($"{tradingStatus}".Contains("X")) BG= Brushes.OrangeRed;
                FG = Streaming.setfg(delay);
                //OnPropertyChanged("Ago");
            }
        }

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Brush BG
        {
            get;
            set;
        }
        public Brush FG
        {
            get;set;
        }

        public virtual void Copy(LastPrice lp)
        {
            this.ShortName = lp.ShortName;
            this.Price = lp.Price;
            this.Date = lp.Date;
            this.TradingStatus = lp.tradingStatus;
        }
    }

    public class LastPriceNew : LastPrice
    {
        public string id { get; set; }


        public double bid { get; set; }
        public double ask { get; set; }
        public double percentageChange { get; set; }

        public string instrumentCurrency { get; set; }
        public string baseCurrency { get; set; }
        public double instrumentToSystemRate { get; set; }
        public double baseToSystemRate { get; set; }

        public double open { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double volume { get; set; }

        public virtual void Copy(LastPriceNew l)
        {
            this.Date = l.Date;
            this.Price = l.Price;
            this.ShortName = l.ShortName;
            this.id = l.id;
            this.bid = l.bid;
            this.ask = l.ask;
            this.percentageChange = l.percentageChange;
            this.instrumentCurrency = l.instrumentCurrency;
            this.baseCurrency = l.baseCurrency;
            this.instrumentToSystemRate = l.instrumentToSystemRate;
            this.baseToSystemRate = l.baseToSystemRate;
            this.TradingStatus = l.TradingStatus;
            this.open = l.open;
            this.high = l.high;
            this.low = l.low;
            this.volume = l.volume;
        }
    }
}

