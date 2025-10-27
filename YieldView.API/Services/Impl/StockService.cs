using Microsoft.Extensions.Options;
using YieldView.API.Configurations;
using YieldView.API.Data;
using YieldView.API.Models;
using YieldView.API.Services.Contract;

namespace YieldView.API.Services.Impl;

public class StockService(IHttpClientFactory httpClientFactory, IOptions<YieldCurveSourcesConfig> options, IServiceScopeFactory scopeFactory, ICSVStockParser stockParser, ILogger<StockService> logger)
  : BackgroundService, IStockService
{
  private readonly YieldCurveSourcesConfig sources = options.Value;
  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

    var httpClient = httpClientFactory.CreateClient("default");
    var fetchInterval = DataFetchHelper.GetDelayForInterval(FetchInterval.Daily);

    while (!cancellationToken.IsCancellationRequested)
    {
      dbContext.BiduPrices.RemoveRange(dbContext.BiduPrices);
      await dbContext.SaveChangesAsync(cancellationToken);

      await HandleSource<BiduStockPrice>(cancellationToken, "Bidu", dbContext, httpClient);
      await HandleSource<PlugStockPrice>(cancellationToken, "PLUG.US", dbContext, httpClient);
      await HandleSource<PorscheAGStockPrice>(cancellationToken, "PorscheAG", dbContext, httpClient);

      await Task.Delay(fetchInterval, cancellationToken);
    }
  }

  private async Task HandleSource<TEntity>(CancellationToken cancellationToken, string stockName, YieldDbContext dbContext, HttpClient httpClient)
    where TEntity : StockPrice, new()
  {
    if (!sources.TryGetValue(stockName, out var stockSource))
    {
      logger.LogError("No {stockName} source configured.", stockName);
      return;
    }
    var url = $"{stockSource.BaseUrl}";

    dbContext.Set<TEntity>().RemoveRange(dbContext.Set<TEntity>());
    await dbContext.SaveChangesAsync(cancellationToken);

    string? csv = await httpClient.GetStringAsync(url, cancellationToken);

    if (string.IsNullOrEmpty(csv))
    {
      throw new InvalidOperationException($"No {stockName} data fetched");
    }
    var parsed = stockParser.Parse<TEntity>(csv);
    dbContext.Set<TEntity>().AddRange(parsed);

    await dbContext.SaveChangesAsync(cancellationToken);
    logger.LogInformation("finished loading  {stockName} data!", stockName);

  }
}
