using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace serviceBusQ
{
    class ServiceBusClient
    {
        List<NamespaceManager> managers = new List<NamespaceManager>();

        List<string> pathsToFiltering = null; 

        public ServiceBusClient(ServiceBusCfg cfg)
        {
            if (cfg is null)
            {
                throw new ArgumentNullException(nameof(cfg));
            }

            cfg.Production
                .ToList()
                .ForEach(e => managers.Add(NamespaceManager.CreateFromConnectionString(e)));
            
        }

        public List<QueueDescription> GetQueueDescriptions()
        {
            List<QueueDescription> queueDescriptions = new List<QueueDescription>();
            Console.WriteLine("queues count " + queueDescriptions.Count);

            managers.ForEach(manager => 
            {
                try
                {
                    queueDescriptions.AddRange(manager.GetQueues());
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });

            if (pathsToFiltering.Count < 1)
            {
                List<string> ps = new List<string>();
                queueDescriptions.ForEach(p => ps.Add(p.Path));
                pathsToFiltering = GetLastVetsionsQueues(ps);
                Console.WriteLine("\n\npath to filtering");
                pathsToFiltering.ForEach(p => Console.WriteLine(p));
            }

            return queueDescriptions;
            //.Where(qd => pathsToFiltering.Any(ptf => ptf == qd.Path)).ToList();


        } 

        private List<string> GetLastVetsionsQueues(List<string> queuePaths) 
        {
            List<string> paths = new List<string>();
            Console.WriteLine("\n\n");
            queuePaths.ForEach(p => Console.WriteLine(p));


            queuePaths.ForEach(path =>
            {
                var inPathsPath = paths.Find(p=> path.Contains(p.Substring(p.IndexOf('-')+1)));

                if (inPathsPath != null && inPathsPath.CompareTo(path) < 0)
                {
                    var index = paths.IndexOf(inPathsPath);
                    Console.WriteLine($"remove {inPathsPath}");
                    paths.RemoveAt(index);
                }
                Console.WriteLine($"add {path}"); 
                paths.Add(path);
                
            });

            return paths;
        }
    }
}
