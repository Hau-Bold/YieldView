using Microsoft.EntityFrameworkCore;
using YieldView.API.Data;
using YieldView.API.Models;

namespace YieldView.API.Services.Impl;

public class SP500DataProvider(IServiceScopeFactory scopeFactory)
{
  private readonly IServiceScopeFactory scopeFactory = scopeFactory;

  public async Task<List<SP500Price>> GetHistoricalPricesAsync(DateTime from, DateTime to)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

    return await dbContext.SP500Prices
                          .Where(p => p.Date >= from && p.Date <= to)
                          .OrderBy(p => p.Date)
                          .ToListAsync();
  }
}

