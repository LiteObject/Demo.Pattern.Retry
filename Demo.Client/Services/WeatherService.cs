using Polly;
using Polly.Retry;
using System.Net;

namespace Demo.Client.Services
{
    public class WeatherService : IWeatherService
    {
        private const int MAX_RETRIES = 3;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AsyncRetryPolicy retryPolicy;

        public WeatherService(IHttpClientFactory httpClientFactory, IRetryDelayCalculator retryDelayCalculator)
        {
            _httpClientFactory = httpClientFactory;

            retryPolicy = Policy.Handle<HttpRequestException>(ex => ex.StatusCode == HttpStatusCode.TooManyRequests)
                    .WaitAndRetryAsync(
                    retryCount: MAX_RETRIES,
                    sleepDurationProvider: retryDelayCalculator.Calculate,
                    onRetry: (exception, sleepDuration, attemptNumber, context) =>
                    {                        
                        Utility.LogWarning($">>> Too many requests. Retrying in {sleepDuration}. {attemptNumber} / {MAX_RETRIES}");
                    });
        }

        public async Task<string> GetForecast()
        {
            var httpClient = _httpClientFactory.CreateClient("UserService");

            return await retryPolicy.ExecuteAsync(async () => 
            {
                var response = await httpClient.GetAsync("/WeatherForecast");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            });
        }
    }
}