using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Client
{
    /// <summary>
    /// Original Source: https://makolyte.com/csharp-how-to-use-polly-to-do-retries/
    /// 
    /// Exponential backoff with jitter spreads out retry attempts so that you’re not sending all of the 
    /// retry attempts at once. It reduces pressure on the server, which decreases the chances of running 
    /// into transient errors.
    /// </summary>
    public class ExponentialBackoffWithJitterCalculator : IRetryDelayCalculator
    {
        private readonly Random random;
        private readonly object randomLock;

        public ExponentialBackoffWithJitterCalculator()
        {
            random = new Random();
            randomLock = new object();
        }

        public TimeSpan Calculate(int attemptNumber)
        {
            int jitter = 0;

            //because Random is not threadsafe
            lock (randomLock)
            {
                jitter = random.Next(10, 200);
            }

            // Calculation: (1 second * 2^attemptCount-1) + random jitter between 10-200ms.
            return TimeSpan.FromSeconds(Math.Pow(2, attemptNumber - 1)) + TimeSpan.FromMilliseconds(jitter);
        }
    }
}
