using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotingInterfaceV2
{
    public interface IMyService : IService
    {
        Task<string> HelloWorldAsync();
    }
}
