using Microsoft.EntityFrameworkCore;
using YieldView.API.Data;
using YieldView.API.Models;

namespace YieldView.API.Services.Impl.Providers;

public class FREDDataProvider(IServiceScopeFactory scopeFactory)
{
  public async Task<List<GDPPrice>> GetGDPPricesAsync(DateTime from, DateTime to)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();
    var gdpPrices = dbContext.GDPPrices.AsQueryable();

    return await GetGdpPricesAsync(from, to, gdpPrices);
  }

  public async Task<List<WilshirePrice>> GetW5000PricesAsync(DateTime from, DateTime to)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();
    var wilshirePrices = dbContext.WilshirePrices.AsQueryable();

    return await GetWilshirePricesAsync(from, to, wilshirePrices);
  }

  private static async Task<List<WilshirePrice>> GetWilshirePricesAsync(
      DateTime from,
      DateTime to,
      IQueryable<WilshirePrice> query
  )
  {
    return await query
        .Where(p => p.Date >= from && p.Date <= to)
        .OrderBy(p => p.Date)
        .ToListAsync();
  }

   private static async Task<List<GDPPrice>> GetGdpPricesAsync(
      DateTime from,
      DateTime to,
      IQueryable<GDPPrice> query
  )
  {
    return await query
        .Where(p => p.Date >= from && p.Date <= to)
        .OrderBy(p => p.Date)
        .ToListAsync();
  }

  public async Task<List<BuffettIndicator>> GetBuffettIndicatorAsync(DateTime from, DateTime to)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();


    var wilshireData = await dbContext.WilshirePrices
        .Where(p => p.Date >= from && p.Date <= to)
        .OrderBy(p => p.Date)
        .ToListAsync();

    var gdpData = await dbContext.GDPPrices
        .OrderBy(p => p.Date)
        .ToListAsync();

    var indicatorPoints = new List<BuffettIndicator>();

    foreach (var w in wilshireData)
    {
      var gdp = gdpData.LastOrDefault(g => g.Date <= w.Date);
      if (gdp != null && gdp.Value > 0)
      {
        indicatorPoints.Add(new BuffettIndicator
        {
          Date = w.Date,
          Value = (w.Value / gdp.Value) * 100.0
        });
      }
    }

    return indicatorPoints;
  }

}
