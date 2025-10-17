
using Microsoft.Extensions.Options;
using YieldView.API.Configurations;
using YieldView.API.Data;
using YieldView.API.Models;
using YieldView.API.Services.Contract;

namespace YieldView.API.Services.Impl;

public class GrossDomesticProductService(IHttpClientFactory httpClientFactory, IOptions<YieldCurveSourcesConfig> options, IServiceScopeFactory scopeFactory, IGDPParser gdpParser,ILogger<GrossDomesticProductService> logger)
  : BackgroundService
{
  private readonly YieldCurveSourcesConfig sources = options.Value;
  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

    if (!sources.TryGetValue("GDP", out var gdpSource))
    {
      logger.LogError("No GDP source configured.");
      return;
    }

    var httpClient = httpClientFactory.CreateClient("default");
    var fetchInterval = DataFetchHelper.GetDelayForInterval(gdpSource.FetchInterval);
    var url = $"{gdpSource.BaseUrl}";

    while (!cancellationToken.IsCancellationRequested)
    {
        dbContext.GDPPrices.RemoveRange(dbContext.GDPPrices);
      await dbContext.SaveChangesAsync(cancellationToken);

      string? csv = await httpClient.GetStringAsync(url, cancellationToken);

      if (string.IsNullOrEmpty(csv))
      {
        throw new InvalidOperationException("No Bidu data fetched");
      }

      dbContext.GDPPrices.AddRange(gdpParser.Parse<GDPPrice>(csv));

      await dbContext.SaveChangesAsync(cancellationToken);
      logger.LogInformation("finished loading BIDU data!");

      await Task.Delay(fetchInterval, cancellationToken);
    }
  }
}
