using Microsoft.ServiceFabric.Services.Remoting.Client;
using RemotingInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemotingInterfaceV2;
using System.ServiceModel.Channels;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System.Fabric;
using WcfInterface;

namespace MyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            CallWcf();
            Test(() => CallRemotingV2(), "V2");
            Test(() => CallRemotingV1(), "V1");           

            Console.ReadLine();
        }

        private static void Test(Func<Task<string>> callSf, string name)
        {
            var totalMs = 0.0;
            int count = 1;
            for (int i = 0; i < 1000; i++)
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var messageV1 = callSf().Result;
                stopWatch.Stop();
                var elapsed = stopWatch.Elapsed.TotalMilliseconds;
                if (i > 10)
                {
                    totalMs += elapsed;
                    count++;
                }
                var avg = totalMs / (count);
                Console.WriteLine($"{name} {messageV1} {i + 1}: {elapsed}ms, Avg:{avg}ms");
            }
        }

        private static async Task<string> CallRemotingV1()
        {
            var helloWorldClient = ServiceProxy.Create<RemotingInterface.IMyService>(new Uri("fabric:/Application1/Stateless1"));

            return await helloWorldClient.HelloWorldAsync();
        }

        private static async Task<string> CallRemotingV2()
        {
            var helloWorldClient = ServiceProxy.Create<RemotingInterfaceV2.IMyService>(new Uri("fabric:/Application1/StatelessV2Stack"));

            return await helloWorldClient.HelloWorldAsync();
        }

        static void CallWcf()
        {
            var ServiceUri = new Uri("fabric:/Application1/Stateful1");
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
            for (int i = 0; i < 1000; i++)
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
