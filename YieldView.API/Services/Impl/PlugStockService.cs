using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YieldView.API.Configurations;
using YieldView.API.Data;
using YieldView.API.Models;
using YieldView.API.Services.Contract;

namespace YieldView.API.Services.Impl;

public class PlugStockService(IHttpClientFactory httpClientFactory, IOptions<YieldCurveSourcesConfig> options, IServiceScopeFactory scopeFactory, ICSVStockParser stockParser,ILogger<PlugStockService>logger)
  : BackgroundService
{
  private readonly YieldCurveSourcesConfig sources = options.Value;
  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

    if (!sources.TryGetValue("PLUG.US", out var sp500Source))
    {
      logger.LogError("No PLUG source configured.");
      return;
    }

    var httpClient = httpClientFactory.CreateClient("default");
    var fetchInterval = DataFetchHelper.GetDelayForInterval(sp500Source.FetchInterval);
    var url = $"{sp500Source.BaseUrl}";

    while (!cancellationToken.IsCancellationRequested)
    {
      dbContext.PlugPrices.RemoveRange(dbContext.PlugPrices);
      await dbContext.SaveChangesAsync(cancellationToken);

      string? csv = await httpClient.GetStringAsync(url, cancellationToken);

      if(string.IsNullOrEmpty(csv))
      {
        throw new InvalidOperationException("No PLUG data fetched");
      }

      dbContext.PlugPrices.AddRange(stockParser.Parse<PlugStockPrice>(csv));

      await dbContext.SaveChangesAsync(cancellationToken);
      logger.LogInformation("finished loading PLUG data!");

      await Task.Delay(fetchInterval, cancellationToken);
    }
  }
}
