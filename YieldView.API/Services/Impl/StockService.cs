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

      await HandleSource<BiduStockPrice>("Bidu", dbContext, httpClient, cancellationToken);
      await HandleSource<PlugStockPrice>("PLUG.US", dbContext, httpClient, cancellationToken);
      await HandleSource<PorscheAGStockPrice>("PorscheAG", dbContext, httpClient, cancellationToken);
      await HandleSource<AlibabaStockPrice>("Alibaba", dbContext, httpClient, cancellationToken);
      await HandleSource<ConstellationBrandsStockPrice>("ConstellationBrands", dbContext, httpClient, cancellationToken);
      await HandleSource<KenvueStockPrice>("Kenvue", dbContext, httpClient, cancellationToken);
      await HandleSource<LyondellBasellStockPrice>("LyondellBasell", dbContext, httpClient, cancellationToken);
      await HandleSource<EastmanChemicalStockPrice>("EastmanChemical", dbContext, httpClient, cancellationToken);
      await HandleSource<DowIncStockPrice>("DowInc", dbContext, httpClient, cancellationToken);
      await HandleSource<RheinmetallStockPrice>("Rheinmetall", dbContext, httpClient, cancellationToken);

      await Task.Delay(fetchInterval, cancellationToken);
    }
  }

  private async Task HandleSource<TEntity>(string stockName, YieldDbContext dbContext, HttpClient httpClient, CancellationToken cancellationToken)
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
