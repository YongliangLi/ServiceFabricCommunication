using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WcfModel
{
    [ServiceContract]
    public interface ICalculator
    {
        [OperationContract]
        Task<int> Add(int value1, int value2);
    }
}
