using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Client
{
    internal class AsyncRetryPolicies: IAsyncRetryPolicies
    {
        private readonly ILogger<AsyncRetryPolicies> _logger;
        private readonly IRetryDelayCalculator _retryDelayCalculator;
        private const int MAX_RETRIES = 3;

        public AsyncRetryPolicies(ILogger<AsyncRetryPolicies> logger, IRetryDelayCalculator retryDelayCalculator)
        {
            this._logger = logger;
            this._retryDelayCalculator = retryDelayCalculator ?? throw new ArgumentNullException(nameof(retryDelayCalculator));
        }

        public IAsyncPolicy<HttpResponseMessage> Get() 
        {
            var retryAttemptCount = 0;

            var retryPolicy =
                //Policy
                Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .RetryAsync(
                retryCount: MAX_RETRIES,
                onRetry: (exception, retryCount, context) =>
                {
                    retryAttemptCount++;
                    _logger.LogInformation($">>> OnRetry: {exception.Exception.Message}. Retrying {retryCount}...");
                });

            var waitAndRetryPolicy =
                    Policy<HttpResponseMessage>
                    .Handle<HttpRequestException>(ex => ex.StatusCode == HttpStatusCode.TooManyRequests)
                    .WaitAndRetryAsync(
                    retryCount: MAX_RETRIES,
                    // This wait time calculation can be either very simple and more complex like exponential backoff with jitter strategy
                    // https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-retries-exponential-backoff
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    // To executing logic between retries use onRetry:
                    // This "onRetry" can be used to fix the problem before the next retry attempt
                    onRetry: (exception, sleepDuration, attemptNumber, context) =>
                    {
                        _logger.LogInformation($">>> OnWaitAndRetry: Too many requests. Retrying in {sleepDuration}. {attemptNumber} / {MAX_RETRIES}");
                    });

            var fallbackPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>(r => r.StatusCode != HttpStatusCode.OK)
                    .FallbackAsync(FallbackAction, OnFallbackAsync);

            /*
             * A request that is executed by the policy wrap will pass through each 
             * policy, and the response will return through each in reverse order. 
             * request -> fallback -> request -> retry
             * fallback <- response <- retry <- response
             */
            return Policy.WrapAsync(fallbackPolicy, waitAndRetryPolicy, retryPolicy);
        }

        private Task<HttpResponseMessage> FallbackAction(CancellationToken arg)
        {
            _logger.LogDebug($">>> Invoked {nameof(FallbackAction)}");

            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($">>> The fallback executed.")
            };

            return Task.FromResult(httpResponseMessage);
        }

        private Task OnFallbackAsync(DelegateResult<HttpResponseMessage> arg)
        {
            _logger.LogDebug($">>> Invoked {nameof(OnFallbackAsync)}");
            return Task.CompletedTask;
        }
    }
}
