using Microsoft.EntityFrameworkCore;
using YieldView.API.Data;
using YieldView.API.Models;

namespace YieldView.API.Services.Impl.Providers;

public class GDPDataProvider(IServiceScopeFactory scopeFactory)
{
  public async Task<List<GDPPrice>> GetGDPPricesAsync(DateTime from, DateTime to)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();
    var gdpPrices = dbContext.GDPPrices.AsQueryable();

    return await GetGDPPricesAsync(from, to, gdpPrices);
  }

  private static async Task<List<GDPPrice>> GetGDPPricesAsync(DateTime from, DateTime to, IQueryable<GDPPrice> dbStockPrices)
  {
    return await dbStockPrices.Where(p => p.Date >= from && p.Date <= to)
                      .OrderBy(p => p.Date)
                      .ToListAsync();
  }
}
