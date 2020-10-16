using Newtonsoft.Json;
using System.IO;

namespace serviceBusQ
{
    public class ServiceBusCfg
    {
        public string[] Production { get; set; }
        
        public string[] Staging { get; set; }
        
        public string[] Critical { get; set; }

        public string[] Major { get; set; }

        public string[] Topics { get; set; }

        static public ServiceBusCfg Create(string configPath)
        {
            using (StreamReader reader = new StreamReader(configPath))
            {
                var r = reader.ReadToEnd();
                var service = JsonConvert.DeserializeObject<ServiceBusCfg>(r);
                return service;
            }
        }
    }
}