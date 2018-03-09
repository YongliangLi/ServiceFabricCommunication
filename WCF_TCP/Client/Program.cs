using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using WcfInterface;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            CallWcf();
            Console.ReadKey();
        }

        static void CallWcf()
        {
            var ServiceUri = new Uri("fabric:/Application3/Stateful1");
            // Create binding
            Binding binding = WcfUtility.CreateTcpClientBinding();

            ServicePartitionResolver servicePartitionResolver = new ServicePartitionResolver(() => new FabricClient());

            // Create a partition resolver
            IServicePartitionResolver partitionResolver = ServicePartitionResolver.GetDefault();
            // create a  WcfCommunicationClientFactory object.
            var wcfClientFactory = new WcfCommunicationClientFactory<ICalculator>
                (clientBinding: binding, servicePartitionResolver: servicePartitionResolver);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            var calculatorServiceCommunicationClient = new WcfCommunicationClient(
                            wcfClientFactory,
                            ServiceUri,
                            new ServicePartitionKey(1));

            //
            // Call the service to perform the operation.
            //
            Stopwatch stopwatch = new Stopwatch();
            var total = 0.0;
            for(int i = 0; i < 1000; i++)
            {
                stopwatch.Restart();
                var result = calculatorServiceCommunicationClient.InvokeWithRetryAsync(
                           client => client.Channel.Add(2, 3)).Result;
                stopwatch.Stop();
                var used = stopwatch.Elapsed.TotalMilliseconds;
                total += used;

                Console.WriteLine($"{i + 1} used: {used}ms, avg:{total / (i + 1)}, result:{result}");

            }

           

        }
    }
}
