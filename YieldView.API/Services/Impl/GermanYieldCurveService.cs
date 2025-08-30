using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Options;
using YieldView.API.Data;
using YieldView.API.Models;
using YieldView.API.Configurations;

namespace YieldView.API.Services.Impl;

public class GermanYieldCurveService : IHostedService
{
  private readonly HttpClient _httpClient;
  private readonly YieldCurveSourcesConfig _sources;
  private readonly IServiceScopeFactory _scopeFactory;

  public GermanYieldCurveService(HttpClient httpClient, IOptions<YieldCurveSourcesConfig> options, IServiceScopeFactory scopeFactory)
  {
    _httpClient = httpClient;
    _sources = options.Value;
    _scopeFactory = scopeFactory;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    if (!_sources.TryGetValue("DE", out var deConfig))
    {
      Console.WriteLine("No configuration found for Germany (DE).");
      return;
    }

    var startYear = deConfig.Years[0];
    var endYear = deConfig.Years.Count > 1 ? deConfig.Years[1] : DateTime.Now.Year;

    using var scope = _scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

    try
    {
      var csvContent = await _httpClient.GetStringAsync(deConfig.BaseUrl, cancellationToken);

      using var reader = new StringReader(csvContent);
      using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

      var records = csv.GetRecords<dynamic>();
      var points = new List<YieldCurvePoint>();

      foreach (var record in records)
      {
        var dict = (IDictionary<string, object>)record;
        if (!dict.TryGetValue("DATE", out var dateObj)) continue;

        if (!DateTime.TryParse(dateObj?.ToString(), out var date)) continue;
        if (date.Year < startYear || date.Year > endYear) continue;

        foreach (var kv in dict)
        {
          if (kv.Key == "DATE") continue;

          if (double.TryParse(kv.Value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var yield))
          {
            points.Add(new YieldCurvePoint
            {
              Country = "DE",
              Date = date,
              Maturity = kv.Key,
              Yield = yield
            });
          }
        }
      }

      await dbContext.DEYieldCurvePoints.AddRangeAsync(points, cancellationToken);
      await dbContext.SaveChangesAsync(cancellationToken);

      Console.WriteLine($"DE Yield-Curve: {points.Count} points loaded for years {startYear}-{endYear}.");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error loading DE Yield-Curve: {ex.Message}");
    }
  }

  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
