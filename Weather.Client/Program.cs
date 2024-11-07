using Microsoft.Extensions.Http.Resilience;

using Polly;
using Polly.Simmy;

using System.Net;

using Weather.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddHostedService<Worker>();

var httpClientBuilder = builder.Services
    .AddHttpClient("weather", client =>
    {
        client.BaseAddress = new Uri("http://localhost:5024");
    });

httpClientBuilder
    .AddStandardResilienceHandler(options =>
    {
        // configure the default rate limiter to allow 3 requests per second
        options.RateLimiter.DefaultRateLimiterOptions.PermitLimit = 3;

        // overall timeout for all retries
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(3);

        // retry 5 times with exponential backoff
        options.Retry.MaxRetryAttempts = 5;
        options.Retry.Delay = TimeSpan.FromMilliseconds(100);
        options.Retry.UseJitter = true;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.OnRetry = static args =>
        {
            Console.WriteLine($"Retry {args.AttemptNumber} after {args.RetryDelay.TotalMilliseconds:F2}ms, due to: {args.Outcome.Result?.StatusCode.ToString() ?? args.Outcome.Exception?.GetType().Name}");
            return default;
        };

        // circuit settings
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(5);
        options.CircuitBreaker.FailureRatio = 0.9;
        options.CircuitBreaker.MinimumThroughput = 5;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(5);

        // timeout for each request
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(1);
    });
    //.AddStandardHedgingHandler()
    //.Configure(options =>
    //{
    //    // configure the default rate limiter to allow 3 requests per second
    //    options.Endpoint.RateLimiter.DefaultRateLimiterOptions.PermitLimit = 3;

//    // overall timeout for all retries
//    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(3);

//    // hedging settings
//    options.Hedging.MaxHedgedAttempts = 5;
//    options.Hedging.Delay = TimeSpan.FromMilliseconds(100);
//    options.Hedging.OnHedging = static args =>
//    {
//        Console.WriteLine($"Hedging attempt {args.AttemptNumber}");
//        return default;
//    };

//    // circuit settings
//    options.Endpoint.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(5);
//    options.Endpoint.CircuitBreaker.FailureRatio = 0.9;
//    options.Endpoint.CircuitBreaker.MinimumThroughput = 5;
//    options.Endpoint.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(5);

//    // timeout for each request
//    options.Endpoint.Timeout.Timeout = TimeSpan.FromSeconds(1);
//});

httpClientBuilder
    .AddResilienceHandler("chaos", (ResiliencePipelineBuilder<HttpResponseMessage> builder) =>
    {
        builder
            .AddChaosLatency(0.2, TimeSpan.FromSeconds(5)) // Add latency to simulate network delays
            .AddChaosFault(0.3, () => new InvalidOperationException("Chaos strategy injection!")) // Inject faults to simulate system errors
            .AddChaosFault(0.2, () => new ArgumentNullException("Chaos strategy injection!")) // Inject faults to simulate system errors
            .AddChaosOutcome(0.3, () => new HttpResponseMessage(HttpStatusCode.InternalServerError)); // Simulate server errors
    });

#region
//.AddStandardHedgingHandler()
//.Configure(options =>
//{
//    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);

//    options.Hedging.MaxHedgedAttempts = 5;
//    options.Hedging.Delay = TimeSpan.FromMilliseconds(50);

//    options.Endpoint.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(5);
//    options.Endpoint.CircuitBreaker.FailureRatio = 0.9;
//    options.Endpoint.CircuitBreaker.MinimumThroughput = 5;
//    options.Endpoint.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(5);

//    options.Endpoint.Timeout.Timeout = TimeSpan.FromSeconds(1);

//})
//.AddResilienceHandler("demo", builder =>
//{
// Custom piepline
//    builder.AddConcurrencyLimiter(100); // this limits the number of concurrent requests to 100

//    builder.AddTimeout(TimeSpan.FromSeconds(5)); // this times out for requesting taking more than 5 second

//    builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage> // this retries 5 times
//    {
//        MaxRetryAttempts = 5,
//        BackoffType = DelayBackoffType.Exponential,
//        UseJitter = true,
//        Delay = TimeSpan.Zero
//    });

//    builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage> // this breaks the circuit after 3 consecutive failures
//    {
//        SamplingDuration = TimeSpan.FromSeconds(5), // sample for 5 seconds
//        FailureRatio = 0.9,
//        MinimumThroughput = 5,
//        BreakDuration = TimeSpan.FromSeconds(5)
//    });

//    builder.AddTimeout(TimeSpan.FromSeconds(1)); // this times out for requesting taking more than 1 second
//})
#endregion


var host = builder.Build();
host.Run();
