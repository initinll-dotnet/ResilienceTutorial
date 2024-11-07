namespace Weather.Server;

public static class WeatherEndpoint
{
    public static void MapWeatherEndpoint(this IEndpointRouteBuilder endpoints)
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        endpoints.MapGet("/weatherforecast", (CancellationToken cancellationToken) =>
        {
            //var random = Random.Shared.NextDouble();

            //if (random < 0.2)
            //{
            //    // Simulate a long running task, 20% of the time
            //    var randomDelay = Random.Shared.Next(5, 20);
            //    var delaySpan = TimeSpan.FromSeconds(randomDelay);

            //    Console.WriteLine($"Server Delay: {delaySpan.Seconds:D2}:{delaySpan.Milliseconds:D3}s");
            //    await Task.Delay(randomDelay, cancellationToken);
            //}
            //else if (random < 0.3)
            //{
            //    // Simulate an error, 30% of the time
            //    Console.WriteLine($"Server Error");
            //    throw new InvalidOperationException("Something went worng");
            //}
            //else
            //{
            //    // normal execution
            //    Console.WriteLine($"Server OK");
            //}

            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast")
        .WithOpenApi();
    }
}