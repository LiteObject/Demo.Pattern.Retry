# A demo project to show Retry pattern using [Polly](https://github.com/App-vNext/Polly) to handle transient failures that are typically self-correcting.

>The Polly .NET library helps simplify retries by abstracting away the retry logic, allowing you to focus on your own code. You can do retries with and without delays.

## What can Polly do?
- Retry failed requests
- Protect your resources
- Prevent from making requests to broken services
- Terminate requests that are taking too long
- Return a default value when all else fails
- Cache previous responses
- It’s thread safe and works on sync and async calls

## How to install Polly?
- $ `dotnet add package Polly --version 7.2.3` (whatever the latest compatible version is)
---
## Retry
Retry policy lets you retry a failed request due to an exception or an unexpected or bad result returned from the called code. It doesn’t wait before retrying.

```
// Retry up to three times in the event of an exception

RetryPolicy policy = Policy.Handle<Exception>().Retry(3);  
policy.Execute(doSomething());
```
```
// Retry if the response is false and you expected true

RetryPolicy<bool> policy = Policy.HandleResult<bool>(b => b != true).Retry(3); 
bool result = policy.Execute(() => GetBool());
```
```
// Do something before retrying

RetryPolicy<HttpResponseMessage> policy = 
    Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
		.RetryAsync(3, onRetry: (response, retryCount) =>
		{
			if (response.Result.StatusCode == HttpStatusCode.Unauthorized)
			{
				PerformReauthorization();
			}
		})
```
## Wait and Retry
The Wait and Retry policy lets you pause before retrying, a great feature for scenarios where all you need is a little time for the problem to resolve. 
## Circuit Breaker
Polly offers two implementations of the circuit breaker: the Basic Circuit Breaker, which breaks when a defined number of consecutive faults occur, and the Advanced Circuit Breaker, which breaks when a threshold of faults occur within a time period, during which a high enough volume of requests were made.

Basic Circuit Breaker example: _The circuit breaks if there are two consecutive failures in a 60-second window._
```
CircuitBreakerPolicy<HttpResponseMessage> basicCircuitBreakerPolicy = Policy
	.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
	 .CircuitBreakerAsync(2, TimeSpan.FromSeconds(60));
 
HttpResponseMessage response = 
	await basicCircuitBreakerPolicy.ExecuteAsync(() =>  
		_httpClient.GetAsync(remoteEndpoint));
```
Advanced Circuit Breaker example: _The circuit breaks for 10 seconds if there is a 1% failure rate in a 60-second window, with a minimum throughput of 1,000 requests._
```
CircuitBreakerPolicy<HttpResponseMessage> advancedCircuitBreakerPolicy = Policy
    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
      .AdvancedCircuitBreakerAsync(0.01, TimeSpan.FromSeconds(60), 1000,
	  TimeSpan.FromSeconds(10));
 
 
HttpResponseMessage response = 
	 await advancedCircuitBreakerPolicy.ExecuteAsync(
	  () => _httpClient.GetAsync(remoteEndpoint));
```

## Fallbacks
Sometimes a request is going to fail no matter how many times you retry. The Fallback policy lets you return some default or perform an action like sending alerts an admin, scaling a system or restarting a service. Fallbacks are generally used in combination with other policies like Retry or Wait and Retry inside a wrap.

```
FallbackPolicy fallbackPolicy = Policy.Handle<Exception>().Fallback(() => EmailAlert());
fallback.Execute(() => GetInventory());

```

## Timeout
The Timeout policy lets you (the caller) decide how long any request should take. If the request takes longer than specified, the policy will terminate the request and cleanup resources via the usage of a cancellation token.

```
// Timeout after one second if no response is received
TimeoutPolicy timeoutPolicy = Policy.Timeout(1, TimeoutStrategy.Pessimistic, OnTimeout);             
var result = timeoutPolicy.Execute(() => ComplexAndSlowCode());
```

## Policy Wraps
When you want to use polices together, use a Policy Wrap. Wraps allow any number of policies to be chained together. In this example, the fallbackPolicy wraps the retryPolicy which wraps the timeoutPolicy.

```
var wrapPolicy = Policy.Wrap(fallbackPolicy, retryPolicy, timeoutPolicy);
wrapPolicy.Execute(() => SomeMethod());
```

## Bulkhead Isolation
Bulkhead Isolation policy lets you control how your application consumes memory, CPU, threads, sockets, et cetera. Even if one part of your application can’t respond, the policy prevents this from bringing down the whole application.

```
// Bulkhead Isolation policy with three execution slots and six queue slots
BulkheadPolicy bulkheadPolicy = Policy.Bulkhead(3, 6); 
var result = bulkheadPolicy.Execute(() => ResourceHeavyRequest());
```

## Cache
Polly’s Cache policy lets you store the results of a previous request in memory or on distributed cache. If a duplicate request is made, Polly will return the stored result from the cache rather than hitting the underlying service a second time.

```
var memoryCache = new MemoryCache(new MemoryCacheOptions());
var memoryCacheProvider = new MemoryCacheProvider(memoryCache);
 
CachePolicy<int> cachePolicy =	Policy.Cache<int>(memoryCacheProvider, TimeSpan.FromSeconds(10)); 
var result = cachePolicy.Execute(context => QueryRemoteService(id), new Context($"QRS-{id}"));
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