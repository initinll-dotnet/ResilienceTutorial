using Spectre.Console;

using System;
using System.Diagnostics;

namespace Weather.Client;

public class Worker : BackgroundService
{
    private readonly HttpClient _httpClient;

    public Worker(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient("weather");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var startTime = Stopwatch.GetTimestamp();

            TimeSpan delta;
            Markup? log = default;

            try
            {
                using var response = await _httpClient.GetAsync("weatherforecast", stoppingToken);
                delta = Stopwatch.GetElapsedTime(startTime);

                var color = ConsoleColor(delta.Seconds, (int)response.StatusCode);

                //log = new Markup($"[{color}]{(int)response.StatusCode}: {delta.Seconds,10:0:00}ms[/]");
                log = new Markup($"[{color}]{(int)response.StatusCode} ({response.StatusCode}) : {delta.Seconds:D2}:{delta.Milliseconds:D3}ms[/]");
            }
            catch (Exception ex)
            {
                delta = Stopwatch.GetElapsedTime(startTime);

                //log = new Markup($"[red]Error: {delta.Seconds,10:0:00}ms ({ex.GetType().Name})[/]");
                log = new Markup($"[red]Error ({ex.GetType().Name}) : {delta.Seconds}:{delta.Milliseconds:D3}ms[/]");
            }

            AnsiConsole.Write(log);
            Console.WriteLine();
        }
    }

    public static string ConsoleColor(int seconds, int statusCode) => (seconds, statusCode) switch
    {
        var (second, status) when second <= 1 && status == 200 => "lime",
        var (second, status) when second > 1 && status == 200 => "white",
        var (second, status) when second <= 1 || status != 200 => "orange1",
        var (second, status) when second > 1 || status != 200 => "white",
        _ => "red",
    };
}
