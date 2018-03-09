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

namespace MyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test(() => CallRemotingV1(), "V1");
            Test(() => CallRemotingV2(), "V2");

            Console.ReadLine();
        }

        private static void Test(Func<Task<string>> callSf, string name)
        {
            var totalMs = 0L;
            for (int i = 0; i < 1000; i++)
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var messageV1 = callSf().Result;
                stopWatch.Stop();
                var elapsed = stopWatch.ElapsedMilliseconds;
                totalMs += elapsed;
                var avg = totalMs / (i + 1);
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

        private static void CallWcf()
        {
            // Create binding
            Binding binding = WcfUtility.CreateTcpClientBinding();
            // Create a partition resolver
            IServicePartitionResolver partitionResolver = ServicePartitionResolver.GetDefault();
            // create a  WcfCommunicationClientFactory object.
            var wcfClientFactory = new WcfCommunicationClientFactory<ICalculator>
                (clientBinding: binding, servicePartitionResolver: partitionResolver);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            var calculatorServiceCommunicationClient = new WcfCommunicationClient(
                            wcfClientFactory,
                            ServiceUri,
                            ServicePartitionKey.Singleton);

            //
            // Call the service to perform the operation.
            //
            var result = calculatorServiceCommunicationClient.InvokeWithRetryAsync(
                            client => client.Channel.Add(2, 3)).Result;
        } 
    }
}
