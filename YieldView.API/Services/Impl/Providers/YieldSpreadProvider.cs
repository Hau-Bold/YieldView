using Microsoft.EntityFrameworkCore;
using YieldView.API.Data;
using YieldView.API.Models;
using YieldView.API.Services.Contract;

namespace YieldView.API.Services.Impl.Providers;

public sealed class YieldSpreadProvider(IServiceScopeFactory scopeFactory) : IYieldSpreadProvider
{
  public async Task<List<YieldSpread>> GetYieldSpreadsAsync(DateTime from, DateTime to, string country)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();
    var query = GetYieldCurvePointsByCountry(country, dbContext);

    var spreads = await query
        .Where(yc => (yc.Maturity == "10Y" || yc.Maturity == "6M")
                     && yc.Date.Date >= from.Date
                     && yc.Date.Date <= to.Date)
        .GroupBy(yc => yc.Date.Date)
        .OrderBy(g => g.Key)
        .Select(g => new YieldSpread
        {
          Date = g.Key,
          TenYear = g.FirstOrDefault(x => x.Maturity == "10Y")!.Yield,
          SixMonth = g.FirstOrDefault(x => x.Maturity == "6M")!.Yield,
          Spread = g.FirstOrDefault(x => x.Maturity == "10Y")!.Yield -
                     g.FirstOrDefault(x => x.Maturity == "6M")!.Yield
        })
        .ToListAsync();

    return spreads;
  }

  private static DbSet<YieldCurvePoint> GetYieldCurvePointsByCountry(string country, YieldDbContext dbContext)
  {
    return country.ToUpper() switch
    {
      "US" => dbContext.USYieldCurvePoints,
      _ => throw new ArgumentException($"Unsupported country: {country}"),
    };
  }
}
