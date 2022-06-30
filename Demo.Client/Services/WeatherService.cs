using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using System.Net;

namespace Demo.Client.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicies;
                
        private readonly IHttpClientFactory _httpClientFactory;

        /* The reason for "static" is circuit breaker relies on a shared state, to track failures across requests. 
         * This can also be accomplished by making the service instance as singleton.*/
        private static AsyncCircuitBreakerPolicy _circuitBreakerPolicy =
            Policy.Handle<HttpRequestException>(ex => 
                ex.StatusCode == HttpStatusCode.Unauthorized || ex.StatusCode == HttpStatusCode.BadRequest)
                    .CircuitBreakerAsync(1, TimeSpan.FromSeconds(1));
        
        // Handle both exceptions and return values in one policy
        HttpStatusCode[] httpStatusCodesWorthRetrying = {
           HttpStatusCode.RequestTimeout, // 408
           HttpStatusCode.InternalServerError, // 500
           HttpStatusCode.BadGateway, // 502
           HttpStatusCode.ServiceUnavailable, // 503
           HttpStatusCode.GatewayTimeout // 504
        };

        public WeatherService(IHttpClientFactory httpClientFactory, IAsyncPolicy<HttpResponseMessage> retryPolicies)
        {
            _httpClientFactory = httpClientFactory;
            _retryPolicies = retryPolicies;
        }

        public async Task<string> GetForecast()
        {
            using var httpClient = _httpClientFactory.CreateClient("WeatherForecastService");

            /*return await retryPolicy.ExecuteAsync(async () => 
            {
                var response = await httpClient.GetAsync("/WeatherForecast");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }); */

            var response = await _retryPolicies.ExecuteAsync(async () =>
            {
                var response = await httpClient.GetAsync("/WeatherForecast");
                response.EnsureSuccessStatusCode();
                return response;
            });

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}