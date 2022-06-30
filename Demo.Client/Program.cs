using System.Net;
using Hellang.Middleware.ProblemDetails;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System.Linq;
using Demo.Client.Services;
using Demo.Client;

ServiceCollection services = new();
ServiceProvider Provider;
ConfigureServices(services);

var weatherService = Provider.GetRequiredService<IWeatherService>();
var forecast = await weatherService.GetForecast();
Console.WriteLine($"Weather forrecast: {forecast}");

Console.WriteLine("\n\nCompleted. Press any key to exit.");

void ConfigureServices(IServiceCollection services)
{

    IAsyncPolicy<HttpResponseMessage> retryPolicy =
       Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
           .RetryAsync(3);
    services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(retryPolicy);

    services.AddScoped<IRetryDelayCalculator, ExponentialBackoffWithJitterCalculator>();
    services.AddSingleton<IAsyncRetryPolicies, AsyncRetryPolicies>();

    services.AddHttpClient();

    services.AddHttpClient("UserService", httpClient =>
    {
        httpClient.BaseAddress = new Uri("https://randomuser.me/api/");
    });

    services.AddHttpClient("WeatherForecastService", httpClient => {
        httpClient.BaseAddress = new Uri("https://localhost:5003");
    });

    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IWeatherService, WeatherService>();

    Provider = services.BuildServiceProvider();
}
