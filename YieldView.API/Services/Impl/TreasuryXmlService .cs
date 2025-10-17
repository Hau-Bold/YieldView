using System.Globalization;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using YieldView.API.Configurations;
using YieldView.API.Data;
using YieldView.API.Models;

namespace YieldView.API.Services.Impl;
public class TreasuryXmlService(IHttpClientFactory httpClientFactory, IOptions<YieldCurveSourcesConfig> options, IServiceScopeFactory scopeFactory, ILogger<TreasuryXmlService> logger)
  : BackgroundService
{
  private readonly YieldCurveSourcesConfig _sources = options.Value;

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

    if (!_sources.TryGetValue("US", out var usSource))
    {
      logger.LogError("US source not configured.");
      return;
    }
    var fetchInterval = DataFetchHelper.GetDelayForInterval(usSource.FetchInterval);
    var httpClient = httpClientFactory.CreateClient("default");

    while (!cancellationToken.IsCancellationRequested)
    {
      dbContext.USYieldCurvePoints.RemoveRange(dbContext.USYieldCurvePoints);
      await dbContext.SaveChangesAsync(cancellationToken);

      foreach (var year in usSource.Years)
      {
        var fullUrl = $"{usSource.BaseUrl}={year}";
        try
        {
          var points = await DownloadAndParseYieldCurveAsync(httpClient,"US", fullUrl);
          await dbContext.USYieldCurvePoints.AddRangeAsync(points, cancellationToken);
          await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
          logger.LogError("Error loading US data for year {Year}: {Message}",year,ex.Message);
        }

        logger.LogInformation("Finished loading US data for year {Year}",year);
      }

      await Task.Delay(fetchInterval, cancellationToken);
    }
  }

  private async Task<List<YieldCurvePoint>> DownloadAndParseYieldCurveAsync(HttpClient httpClient,string country, string url)
  {
    var xml = await httpClient.GetStringAsync(url);
    var doc = XDocument.Parse(xml);

    XNamespace atom = "http://www.w3.org/2005/Atom";
    XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
    XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

    var entries = doc.Descendants(atom + "entry");
    var result = new List<YieldCurvePoint>();

    foreach (var entry in entries)
    {
      var props = entry.Descendants(m + "properties").FirstOrDefault();
      if (props == null) continue;

      var dateStr = props.Element(d + "NEW_DATE")?.Value;
      if (!DateTime.TryParse(dateStr, out var date)) continue;

      foreach (var element in props.Elements())
      {
        var localName = element.Name.LocalName;
        if (!localName.StartsWith("BC_") || localName == "BC_30YEARDISPLAY")
          continue;

        var maturity = localName.Replace("BC_", "")
                                .Replace("YEAR", "Y")
                                .Replace("MONTH", "M");

        if (double.TryParse(element.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var yield))
        {
          result.Add(new YieldCurvePoint
          {
            Country = country,
            Date = date,
            Maturity = maturity,
            Yield = yield
          });
        }
      }
    }

    return result;
  }
}
