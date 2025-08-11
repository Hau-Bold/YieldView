using System.Globalization;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using YieldView.API.Configurations;
using YieldView.API.Data;
using YieldView.API.Models;

namespace YieldView.API.Services.Impl;
public class TreasuryXmlService : IHostedService
{
  private readonly HttpClient _httpClient;
  private readonly YieldCurveSourcesConfig _sources;
  private readonly IServiceScopeFactory _scopeFactory;
  
  public TreasuryXmlService(HttpClient httpClient, IOptions<YieldCurveSourcesConfig> options, IServiceScopeFactory scopeFactory)
  {
    _httpClient = httpClient;
    _sources = options.Value;
    _scopeFactory = scopeFactory;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    using var scope = _scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

    foreach (var (countryCode, source) in _sources)
    {
      foreach (var year in source.Years)
      {
        var fullUrl = $"{source.BaseUrl}={year}";
        try
        {
          var points = await DownloadAndParseYieldCurveAsync(countryCode, fullUrl);

          await dbContext.YieldCurvePoints.AddRangeAsync(points,cancellationToken);
          await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Error loading data for {countryCode} year {year}: {ex.Message}");
        }

         Console.WriteLine($"Finished loading data for {countryCode} year {year}");
      }
    }


  }

  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

  public async Task<List<YieldCurvePoint>> DownloadAndParseYieldCurveAsync(string country, string url)
  {
    var xml = await _httpClient.GetStringAsync(url);
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
