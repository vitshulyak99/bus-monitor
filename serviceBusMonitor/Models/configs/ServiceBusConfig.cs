using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace serviceBusMonitor.Models
{
    class ServiceBusConfig
    {
        public string[] Production { get; set; }
        public string[] Staging { get; set; }
        public string[] Critical { get; set; }
        public string[] Major { get; set; }

        public string[] Topics { get; set; }

        static public ServiceBusConfig Create(string configPath)
        {
            using (StreamReader reader = new StreamReader(configPath))
            {
                var r = reader.ReadToEnd();
                var service = JsonConvert.DeserializeObject<ServiceBusConfig>(r);
                return service;
            }
        }

        

    }

    
}
