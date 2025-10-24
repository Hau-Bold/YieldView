
using Microsoft.Extensions.Options;
using YieldView.API.Configurations;
using YieldView.API.Data;
using YieldView.API.Models;
using YieldView.API.Services.Contract;

namespace YieldView.API.Services.Impl;

public class BiduStockService(IHttpClientFactory httpClientFactory, IOptions<YieldCurveSourcesConfig> options, IServiceScopeFactory scopeFactory, ICSVStockParser stockParser, ILogger<BiduStockService> logger)
  : BackgroundService, IBiduStockService
{
  private readonly YieldCurveSourcesConfig sources = options.Value;
  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

    if (!sources.TryGetValue("Bidu", out var sp500Source))
    {
      logger.LogError("No Bidu source configured.");
      return;
    }

    var httpClient = httpClientFactory.CreateClient("default");
    var fetchInterval = DataFetchHelper.GetDelayForInterval(sp500Source.FetchInterval);
    var url = $"{sp500Source.BaseUrl}";

    while (!cancellationToken.IsCancellationRequested)
    {
      dbContext.BiduPrices.RemoveRange(dbContext.BiduPrices);
      await dbContext.SaveChangesAsync(cancellationToken);

      string? csv = await httpClient.GetStringAsync(url, cancellationToken);

      if (string.IsNullOrEmpty(csv))
      {
        throw new InvalidOperationException("No Bidu data fetched");
      }

      dbContext.BiduPrices.AddRange(stockParser.Parse<BiduStockPrice>(csv));

      await dbContext.SaveChangesAsync(cancellationToken);
      logger.LogInformation("finished loading BIDU data!");

      await Task.Delay(fetchInterval, cancellationToken);
    }
  }
}
