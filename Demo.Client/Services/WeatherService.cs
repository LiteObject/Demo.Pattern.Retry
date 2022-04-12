using Polly;
using Polly.Retry;
using System.Net;

namespace Demo.Client.Services
{
    public class WeatherService : IWeatherService
    {
        private const int MAX_RETRIES = 3;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AsyncRetryPolicy<HttpResponseMessage>? retryPolicy;

        public WeatherService(IHttpClientFactory httpClientFactory, IRetryDelayCalculator retryDelayCalculator)
        {
            _httpClientFactory = httpClientFactory;
            retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(
                    retryCount: MAX_RETRIES,
                    onRetry: (response, retryCount) =>
                    {
                        Utility.LogInfo($">>> Http Response: {response.Result.StatusCode}");

                        if (response.Result.StatusCode == HttpStatusCode.GatewayTimeout)
                        {
                            //....
                        }
                    });
        }

        public async Task<string> GetForecast()
        {
            var httpClient = _httpClientFactory.CreateClient("UserService");

            await retryPolicy.ExecuteAsync(async () => 
            {
                var response = await httpClient.GetAsync($"/WeatherForecast");
                var forecast = await response.Content.ReadAsStringAsync();
            });
        }
    }
}