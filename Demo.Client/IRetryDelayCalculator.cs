using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Client
{
    public interface IRetryDelayCalculator
    {
        public TimeSpan Calculate(int attemptNumber);
    }
}
