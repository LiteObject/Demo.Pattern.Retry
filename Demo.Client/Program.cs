using System.Net;
using Hellang.Middleware.ProblemDetails;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System.Linq;
using Demo.Client.Services;
using Demo.Client;
using Microsoft.Extensions.Logging;

ServiceCollection services = new();
ServiceProvider Provider;
ConfigureServices(services);

var logger = Provider.GetRequiredService<ILogger<Program>>();
logger.LogInformation("console application started...");

var weatherService = Provider.GetRequiredService<IWeatherService>();

try
{
    var forecast = await weatherService.GetForecast();
    logger.LogInformation($"*** Weather forrecast: {forecast} ***");
}
catch (Exception e)
{
    logger.LogError(e, e.Message);
}

// Console.WriteLine("\n\nCompleted. Press any key to exit.");
logger.LogInformation("\n\nCompleted. Press any key to exit.");
Console.ReadLine();

void ConfigureServices(IServiceCollection services)
{
    services.AddLogging(builder => 
    {
        builder.AddConsole();        
    }).Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Debug);

    /*IAsyncPolicy<HttpResponseMessage> retryPolicy =
       Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
           .RetryAsync(3); */

    // services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(retryPolicy);

    services.AddSingleton<IRetryDelayCalculator, ExponentialBackoffWithJitterCalculator>();
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
