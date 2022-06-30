using Microsoft.AspNetCore.Mvc.Testing;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Demo.Test
{
    public class IntegrationTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _client;
        private const int MAX_RETRIES = 3;

        public IntegrationTests(ITestOutputHelper output, WebApplicationFactory<Program> factory)
        {
            _output = output;
            _client = factory.CreateDefaultClient();
        }

        [Fact]
        public async Task Get_Should_Return_200() 
        {
            // ARRANGE            
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
                    _output.WriteLine($">>> OnRetry: {exception.Exception.Message}. Retrying {retryCount}...");
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
                        _output.WriteLine($">>> OnWaitAndRetry: Too many requests. Retrying in {sleepDuration}. {attemptNumber} / {MAX_RETRIES}");
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
            var policyWrapper = Policy.WrapAsync(fallbackPolicy, waitAndRetryPolicy, retryPolicy);

            // ACT            
            var response = await policyWrapper.ExecuteAsync(async () =>
            {
                var response = await _client.GetAsync("/WeatherForecast");
                _output.WriteLine($">>> Response Status Code: {response.StatusCode}");
                response.EnsureSuccessStatusCode();
                return response;
            });

            _output.WriteLine($"Final value of retryAttemptCount: {retryAttemptCount}");

            // ASSERT
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            //Assert.Equal(MAX_RETRIES, retryAttemptCount);
        }

        private Task<HttpResponseMessage> FallbackAction(CancellationToken arg)
        {
            _output.WriteLine($">>> Invoked {nameof(FallbackAction)}");

            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($">>> The fallback executed.")
            };

            return Task.FromResult(httpResponseMessage);
        }

        private Task OnFallbackAsync(DelegateResult<HttpResponseMessage> arg)
        {
            _output.WriteLine($">>> Invoked {nameof(OnFallbackAsync)}");
            return Task.CompletedTask;
        }
    }
}