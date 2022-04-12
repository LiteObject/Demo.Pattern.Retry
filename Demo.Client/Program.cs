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

var MAX_RETRIES = 3;

/************************************************************************
 * BASIC EXAMPLE
 ************************************************************************/

// Create a retry policy
var retryPolicy = Policy.Handle<TransientException>()
	.WaitAndRetry(
        retryCount: MAX_RETRIES, 
        // This wait time calculation can be either very simple and more complex like exponential backoff with jitter strategy
        // https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-retries-exponential-backoff
        sleepDurationProvider: (attemptCount) => TimeSpan.FromSeconds(attemptCount * 2),

        // To executing logic between retries use onRetry:
        // This "onRetry" can be used to fix the problem before the next retry attempt
        onRetry: (exception, sleepDuration, attemptNumber, context) =>
        {
            Console.WriteLine($"Transient error: {exception.Message}. Retrying in {sleepDuration}. {attemptNumber} / {MAX_RETRIES}");
        });

// Execute code with the retry policy only if the attempt has a chance of succeeding
retryPolicy.Execute(() =>
{
    var ex = new WebException("An unexpected event closed the connection. Please retry.", WebExceptionStatus.ConnectionClosed);
	throw new TransientException("Some transient exception occured", ex);
});

/************************************************************************
 * API CALL EXAMPLE with WaitAndRetryAsync & Exponential backoff with jitter
 ************************************************************************/
var weatherService = Provider.GetRequiredService<IWeatherService>();
var forecast = weatherService.GetForecast();
Console.WriteLine($"Weather forrecast: {forecast}");

/************************************************************************
 * API CALL EXAMPLE
 ************************************************************************/
var userService = Provider.GetRequiredService<IUserService>();
var users = await userService.GetUsers(10);
users.ForEach(u => Utility.LogInfo($"{u.Name.First} {u.Name.Last}"));

Console.WriteLine("Completed. Press any key to exit.");

void ConfigureServices(IServiceCollection services)
{    
    //services.AddDbContext<AppDbContext>(options =>
    //{
    //    options.UseInMemoryDatabase("test-db");
    //});

    services.AddHttpClient();

    services.AddScoped<IRetryDelayCalculator, ExponentialBackoffWithJitterCalculator>();

    services.AddHttpClient("UserService", httpClient =>
    {
        httpClient.BaseAddress = new Uri("https://randomuser.me/api/");
    });

    services.AddHttpClient("WeatherForecastService", httpClient => {
        httpClient.BaseAddress = new Uri("https://localhost:7080/WeatherForecast");
    });

    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IWeatherService, WeatherService>();

    Provider = services.BuildServiceProvider();
}
