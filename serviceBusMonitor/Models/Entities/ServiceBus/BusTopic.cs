using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serviceBusMonitor.Models.Entities.ServiceBus
{
    public class BusTopic : BusQueue
    {
        public BusTopic(string path, string name , long active, long deadl) : base(path, active, deadl)
        {
            Name = name; 
        }

        public string Name { get; }

    }
}
