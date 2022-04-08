using System.Net;
using Polly;

var MAX_RETRIES = 3;

//Create a retry policy
var retryPolicy = Policy.Handle<TransientException>()
	.WaitAndRetry(
        retryCount: MAX_RETRIES, 
        // This wait time calculation can be either very simple and more complex like exponential backoff with jitter strategy
        // https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-retries-exponential-backoff
        sleepDurationProvider: (attemptCount) => TimeSpan.FromSeconds(attemptCount * 2),
        onRetry: (exception, sleepDuration, attemptNumber, context) =>
        {
            Console.WriteLine($"Transient error: {exception.Message}. Retrying in {sleepDuration}. {attemptNumber} / {MAX_RETRIES}");
        });

//Execute the error prone code with the policy
retryPolicy.Execute(() =>
{
    var ex = new WebException("An unexpected event closed the connection. Please retry.", WebExceptionStatus.ConnectionClosed);
	throw new TransientException("Some transient exception occured", ex);
});

Console.WriteLine("Completed. Press any key to exit.");
