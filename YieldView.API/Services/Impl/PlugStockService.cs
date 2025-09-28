
using Microsoft.Extensions.Options;
using System.Globalization;
using YieldView.API.Configurations;
using YieldView.API.Data;
using YieldView.API.Models;

namespace YieldView.API.Services.Impl;

public class PlugStockService(HttpClient httpClient, IOptions<YieldCurveSourcesConfig> options, IServiceScopeFactory scopeFactory)
  : BackgroundService
{
  private readonly YieldCurveSourcesConfig sources = options.Value;
  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

    if (!sources.TryGetValue("PLUG.US", out var sp500Source))
    {

      Console.WriteLine("No PLUG source configured.");
      return;
    }

    var fetchInterval = DataFetchHelper.GetDelayForInterval(sp500Source.FetchInterval);
    var url = $"{sp500Source.BaseUrl}";

    while (!cancellationToken.IsCancellationRequested)
    {
      dbContext.PlugPrices.RemoveRange(dbContext.PlugPrices);
      await dbContext.SaveChangesAsync(cancellationToken);

      var csv = await httpClient.GetStringAsync(url, cancellationToken);

      var lines = csv.Split("\n").Skip(1);

      foreach (var line in lines)
      {
        if (string.IsNullOrWhiteSpace(line))
        {
          continue;
        }

        var parts = line.Split(',');

        if (DateTime.TryParse(parts[0], out var date) &&
            double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var open) &&
            double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var high) &&
            double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var low) &&
            double.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var close) &&
            long.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var volume)
            )
        {
          dbContext.PlugPrices.Add(new PlugStockPrice
          {
            Date = date,
            Open = open,
            High = high,
            Low = low,
            Close = close,
            Volume = volume
          });
        }
      }

      await dbContext.SaveChangesAsync(cancellationToken);
      Console.WriteLine("finished loading PLUG data!");

      await Task.Delay(fetchInterval, cancellationToken);
    }
  }
}
