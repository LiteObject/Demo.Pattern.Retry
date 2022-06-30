using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Client
{
    internal interface IAsyncRetryPolicies
    {
        public IAsyncPolicy<HttpResponseMessage> Get();
    }
}
