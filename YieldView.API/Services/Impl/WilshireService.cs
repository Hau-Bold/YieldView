using Microsoft.Extensions.Options;
using System.Net.Http;
using YieldView.API.Configurations;
using YieldView.API.Data;
using YieldView.API.Services.Contract;

namespace YieldView.API.Services.Impl;

public class WilshireService(IHttpClientFactory httpClientFactory, IOptions<YieldCurveSourcesConfig> options, IServiceScopeFactory scopeFactory, IWilshireParser wilshireParser,ILogger<WilshireService> logger)
  : BackgroundService
{
  private readonly YieldCurveSourcesConfig sources = options.Value;
  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

    if (!sources.TryGetValue("Wilshire", out var wilshireSource))
    {
      logger.LogError("No Wilshire source configured.");
      return;
    }

    var fetchInterval = DataFetchHelper.GetDelayForInterval(wilshireSource.FetchInterval);
    var url = $"{wilshireSource.BaseUrl}";

    while (!cancellationToken.IsCancellationRequested)
    {
        dbContext.WilshirePrices.RemoveRange(dbContext.WilshirePrices);
      await dbContext.SaveChangesAsync(cancellationToken);


      logger.LogInformation("Fetching Wilshire 5000 data from {Url}", url);

      var httpClient = httpClientFactory.CreateClient("default");
      httpClient.DefaultRequestHeaders.Clear();
      httpClient.DefaultRequestHeaders.Add("User-Agent",
          "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
          "(KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
      httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
      httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

      var rawData = await httpClient.GetStringAsync(url, cancellationToken);

      if (string.IsNullOrWhiteSpace(rawData))
      {
        throw new InvalidOperationException("No Wilshire data fetched");
      }

      dbContext.WilshirePrices.AddRange(wilshireParser.Parse(rawData));

      await dbContext.SaveChangesAsync(cancellationToken);
      logger.LogInformation("finished loading BIDU data!");

      await Task.Delay(fetchInterval, cancellationToken);
    }
  }
}
