using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using System.Windows.Media;

namespace serviceBusMonitor.Models.Entities.ServiceBus
{
    public class BusQueue : INotifyPropertyChanged
    {
        static string[] major;
        static string[] critical;
        List<long> lastFive = new List<long>();
        string _path;
        long active;
        long dead;

        public BusQueue(string path, long active, long deadl)
        {
            this._path = path ?? throw new ArgumentNullException(nameof(path));
            var p = _path.Split('.');
            Worker = p[p.Length - 2];
            Message = p[p.Length - 1];
            this.Active = active;
            this.DeadLetter = deadl;
        }

        public string Path { get => _path; }

        public Brush BackgroundA { get; set; }
        public Brush BackgroundD { get; set; }
        public long Active
        {
            get => active;  set
            {
                active = value;

                if (lastFive.Count >= 5)
                {
                    lastFive.RemoveAt(0);

                }
                lastFive.Add(active);

                BackgroundA = active > 0 && active < 1000 ? Brushes.LightGreen :
                    active > 999 && active < 3000 ? Brushes.Yellow :
                    active > 2999 ? Brushes.OrangeRed : Brushes.White;

            }
        }
        public long DeadLetter
        {
            get => dead;  set
            {
                dead = value;
                if (BusQueue.critical != null)
                {
                    if (critical.Contains(Message))
                    {
                        BackgroundD = dead > 1 ? Brushes.OrangeRed : Brushes.White;
                    }
                }
                else if (BusQueue.major != null)
                {
                    if (BusQueue.major.Contains(Message))
                    {
                        BackgroundD = dead > 10 && dead < 1000 ? Brushes.Yellow :
                            dead > 999 ? Brushes.OrangeRed : Brushes.White;
                    }
                }
                else
                {
                    BackgroundD = dead > 0 && dead < 1000 ? Brushes.LightGreen :
                    dead > 999 && dead < 2000 ? Brushes.Yellow :
                    dead > 1999 ? Brushes.OrangeRed : Brushes.White;
                }
                
            }
        }

        internal void UpdateValues(BusQueue i)
        {
            this.Active = i.Active;
            this.DeadLetter = i.DeadLetter;
        }

        public long Avg
        {
            get
            {
                if (lastFive.Count != 0)
                    return lastFive.Sum() / lastFive.Count;
                else return 0;
            }
        }
        public string Worker
        {
            get;
 
        }

        public string Message
        {
            get;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string name = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        static public void SetMajorCritical(string[] major = null, string[] critical = null)
        {
            BusQueue.major = major;
            BusQueue.critical = critical;
        }
    }

    
}