using serviceBusMonitor.ViewModels;
using System;

using System.Windows.Media;

namespace serviceBusMonitor.Models
{
    public class OrderBook
    {
        private double delay;
        private Brush background;
        private Brush foreground;

        public void Copy(OrderBook ob)
        {
            this.ShortName = ob.ShortName;
            this.Date = ob.date;
        }

        public double Delay
        {
            get { return delay; }
            private set
            {
                delay = value;
                BG = Streaming.setbg(delay);
                FG = Streaming.setfg(delay);

                // OnPropertyChanged("Delay");
            }

        }

        public Brush FG
        {
            get { return foreground; }
            set
            {
                foreground = value;
                // OnPropertyChanged("FG");
            }
        }

        public Brush BG
        {
            get { return background; }
            set
            {
                background = value;
                //  OnPropertyChanged("BG");
            }

        }

        public string ShortName
        {
            get;
            set;
        }
        public DateTime Date { get
            {
            return    date;
            }
            set
            {
                Delay =Math.Round( (DateTime.Now - value).TotalSeconds,4);
                date = value;
               
            }
        }

        private DateTime date;


    }

    internal class OB
    {
        public  string ShortName { get; set; }
        public DateTime Date { get; set; }
    }
}