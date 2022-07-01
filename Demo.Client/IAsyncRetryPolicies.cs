using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Client
{
    public interface IAsyncRetryPolicies
    {
        public IAsyncPolicy<HttpResponseMessage> Get();
    }
}
