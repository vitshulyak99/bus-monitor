using System;

namespace serviceBusQ
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceBusCfg serviceBusCfg = ServiceBusCfg.Create(@"config/serviceBusConfig.json");
            ServiceBusClient serviceBusClient = new ServiceBusClient(serviceBusCfg);

            try
            {

                var queues = serviceBusClient.GetQueueDescriptions();

                queues.ForEach(q => Console.WriteLine(q.Path));

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            Console.ReadLine();
            
        }
    }
}
