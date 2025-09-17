using Microsoft.EntityFrameworkCore;
using YieldView.API.Data;
using YieldView.API.Models;

namespace YieldView.API.Services.Impl;

public class StockDataProvider(IServiceScopeFactory scopeFactory)
{
  public async Task<List<T>> GetStockPricesAsync<T>(DateTime? from = null, DateTime? to = null)
         where T : StockPrice
  {

    if (!from.HasValue || !to.HasValue)
    {
      throw new ArgumentException("Both 'from' and 'to' dates must have a value.");
    }

    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

    var dbSet = dbContext.Set<T>().AsQueryable();

    return await dbSet.Where(p => p.Date >= from && p.Date <= to)
                      .OrderBy(p => p.Date)
                      .ToListAsync();
  }
}
