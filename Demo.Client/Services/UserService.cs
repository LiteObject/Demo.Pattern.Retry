using Demo.Client.Services.Models;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Demo.Client.Services
{
    public class UserService : IUserService
    {
        private const int MAX_RETRIES = 3;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AsyncRetryPolicy retryPolicy;

        public UserService(IHttpClientFactory httpClientFactory, IRetryDelayCalculator retryDelayCalculator)
        {
            _httpClientFactory = httpClientFactory;

            retryPolicy = Policy.Handle<HttpRequestException>(ex => ex.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
               retryCount: MAX_RETRIES,
               sleepDurationProvider: retryDelayCalculator.Calculate,
               onRetry: (exception, sleepDuration, attemptNumber, context) =>
               {
                   Utility.LogInfo($">>> Transient error: {exception.Message}. Retrying in {sleepDuration}. {attemptNumber} / {MAX_RETRIES}");
               });


            var httpRetryWithReauthorizationPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(3, onRetry: (response, retryCount) =>
                    {
                        if (response.Result.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            // ...
                        }
                    });


            var circuitBreakerPolicy = Policy.Handle<TransientException>()
                .CircuitBreaker(exceptionsAllowedBeforeBreaking: 3, durationOfBreak: TimeSpan.FromSeconds(10),
                    onBreak: (_, duration) => Utility.LogInfo($"Circuit open for duration {duration}"),
                    onReset: () => Utility.LogInfo("Circuit closed and is allowing requests through"),
                    onHalfOpen: () => Utility.LogInfo("Circuit is half-opened and will test the service with the next request"));
        }

        public async Task<List<User>> GetUsers(int count = 10)
        {
            List<User> users = new();
            var httpClient = _httpClientFactory.CreateClient("UserService");

            await retryPolicy.ExecuteAsync(async () =>
            {
                var response = await httpClient.GetAsync($"/?results={count}");
                response.EnsureSuccessStatusCode();

                using var contentStream = await response.Content.ReadAsStreamAsync();
                var tempUsers = await JsonSerializer.DeserializeAsync<IEnumerable<User>>(contentStream);

                if (tempUsers?.Count() > 0)
                {
                    users.Clear();
                    users.AddRange(tempUsers);
                }
                else
                {
                    Utility.LogInfo(">>> API call didn't return any users.");
                }
            });

            return users;
        }
    }
}
