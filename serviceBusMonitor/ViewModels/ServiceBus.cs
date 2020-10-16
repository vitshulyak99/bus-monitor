using serviceBusMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ServiceBus;

using Microsoft.ServiceBus.Messaging;
using serviceBusMonitor.Models.Entities.ServiceBus;

namespace serviceBusMonitor.ViewModels
{
    class ServiceBus
    {
        public bool IsRunning { get; private set; }

        ServiceBusConfig Config;
        private DateTime lastUpdate;
        NamespaceManager[] Production = null;
        NamespaceManager[] Staging = null;
        NamespaceManager[] focus = null;
        
        List<BusQueue> queues = new List<BusQueue> ();
        List<BusTopic> topics = new List<BusTopic> ();
        private object switchServerLocker =  new object();

        public ServiceBus(ServiceBusConfig config)
        {
           
            IsRunning = false;
            Config = config;
            Production = Array.ConvertAll(config.Production, (x) => NamespaceManager.CreateFromConnectionString(x));
            Staging = Array.ConvertAll(config.Staging, (x) => NamespaceManager.CreateFromConnectionString(x));
            BusQueue.SetMajorCritical(config.Major, config.Critical);
        }

        public List<BusQueue> GetGetQueues()
        {
            foreach (var n in focus)
            {
                var v = n.GetQueues();
                var x = Map(v.ToList());

                foreach (var i in x)
                {
                    var val = queues.FirstOrDefault(p => p.Message == i.Message);
                    if (val == null)
                        queues.Add(i);
                    else
                        queues[queues.IndexOf(val)].UpdateValues(i);
                }
            }

            return queues;

        }

        public List<BusTopic> GetGetTopics()
        {

            if (focus != null)
            { 
                topics = new List<BusTopic>();
                
                foreach (var n in focus)
                {
                    foreach (var t in Config.Topics)
                    {
                        var v = n.GetSubscriptions(t);

                        var x = Map(v.ToList());


                        topics.AddRange(x);
                      
                    }
                }
                return topics;

            }
            else return null;

        }


        //public IEnumerable<Topics>
        /// <summary>
        /// 0 - production
        /// 1 - staging
        /// other -none
        /// </summary>
        /// <param name="s"></param>
        public void SetServer(int s)
        {
            lock(switchServerLocker)
            switch (s)
            {
                case 0:
                    {
                            focus = Production;
                    }break;
                case 1:
                    {
                            focus = Staging;
                    }break;
                default:
                    {
                            focus = null;
                    }break;
            }
        }

        private List<BusQueue> Map(List<QueueDescription> description)
        {
            List<BusQueue> btq = new List<BusQueue>();
            description.ForEach( (vv) => btq.Add(new BusQueue(vv.Path, vv.MessageCountDetails.ActiveMessageCount, vv.MessageCountDetails.DeadLetterMessageCount)));
            
            
            return btq;
        }

        private List<BusTopic> Map(List<SubscriptionDescription> description)
        {
            List<BusTopic> btq = new List<BusTopic>();
            foreach (var vv in description)
            {
                btq.Add(new BusTopic(vv.TopicPath, vv.Name, vv.MessageCountDetails.ActiveMessageCount, vv.MessageCountDetails.DeadLetterMessageCount));
            }

            return btq;
        }

    }

    
    
}
