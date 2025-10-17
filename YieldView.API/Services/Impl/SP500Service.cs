using System.Globalization;
using Microsoft.Extensions.Options;
using YieldView.API.Configurations;
using YieldView.API.Data;
using YieldView.API.Models;

namespace YieldView.API.Services.Impl;

public class SP500Service(IHttpClientFactory httpClientFactory, IOptions<YieldCurveSourcesConfig> options, IServiceScopeFactory scopeFactory, ILogger<SP500Service> logger)
 : BackgroundService
{
    private readonly YieldCurveSourcesConfig sources = options.Value;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

        if (!sources.TryGetValue("SP500", out var sp500Source))
        {
            return;
        }

        var fetchInterval = DataFetchHelper.GetDelayForInterval(sp500Source.FetchInterval);
        var httpClient = httpClientFactory.CreateClient("default");

    while (!cancellationToken.IsCancellationRequested)
        {
            dbContext.SP500Prices.RemoveRange(dbContext.SP500Prices);
            await dbContext.SaveChangesAsync(cancellationToken);

            var startDate = new DateTime(sp500Source.Years[0], 1, 1).ToString("yyyyMMdd");
            var endDate = DateTime.Today.ToString("yyyyMMdd");

            var url = $"{sp500Source.BaseUrl}&d1={startDate}&d2={endDate}";

            var csv = await httpClient.GetStringAsync(url, cancellationToken);

            var lines = csv.Split("\n").Skip(1);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = line.Split(',');

                    // Todo: Maybe using decimal is better????
                if (DateTime.TryParse(parts[0], out var date) &&
                    double.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var close))
                {
                    dbContext.SP500Prices.Add(new SP500Price
                    {
                        Date = date,
                        Close = close   
                    });
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("finished loading sp500 data!");

            await Task.Delay(fetchInterval, cancellationToken);
        }
    }
}

