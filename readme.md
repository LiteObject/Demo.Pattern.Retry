# A demo project to show Retry pattern, Circuit breaker, etc. using [Polly](https://github.com/App-vNext/Polly) to handle transient failures that are typically self-correcting.

>The Polly .NET library helps simplify retries by abstracting away the retry logic, allowing you to focus on your own code. You can do retries with and without delays.

## Install Polly:
- $ `dotnet add package Polly --version 7.2.3` (whatever the latest compatible version is)
---
## Retry example: Build the policy
```
var retryPolicy = Policy.Handle<TransientException>()
	.WaitAndRetry(retryCount: 3, sleepDurationProvider: _ => TimeSpan.FromSeconds(1));
```

## Retry example: Execute the error prone code with the policy
```
var attempt = 0;
retryPolicy.Execute(() => {
	Log($"Attempt {++attempt}");
	throw new TransientException();
});
```
---

## What is Circuit breaker?
>Circuit breaker is similar to the retry pattern. The difference is the circuit breaker pattern applies to all requests while retries apply to individual requests.

## Circuit states
There are three main circuit states: Closed, Open, and Half-Open. These can be summarized in the following

- Closed: The circuit is allowing requests through.
- Open: The circuit tripped and isn’t allowing requests through right now.
- HalfOpen: The next request that comes through will be used to test the service, while all other requests will be rejected.
- There’s another state called “Isolated”. It’s only used when you manually trip the circuit.

### Log circuit state changes:
`var circuitBreakerPolicy = Policy.Handle<TransientException>()
	.CircuitBreaker(exceptionsAllowedBeforeBreaking: 3, durationOfBreak: TimeSpan.FromSeconds(10),
		onBreak: (_, duration) => Log($"Circuit open for duration {duration}"),
		onReset: () => Log("Circuit closed and is allowing requests through"),
		onHalfOpen: () => Log("Circuit is half-opened and will test the service with the next request"));`

## Links to external resources:
- https://docs.microsoft.com/en-us/azure/architecture/patterns/retry