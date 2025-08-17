using Microsoft.EntityFrameworkCore;
using YieldView.API.Data;
using YieldView.API.Models;

namespace YieldView.API.Services.Impl;

public class SP500DataProvider
{
  private readonly IServiceScopeFactory scopeFactory;

  public SP500DataProvider(IServiceScopeFactory scopeFactory)
  {
    this.scopeFactory = scopeFactory;
  }

  public async Task<List<SP500Price>> GetHistoricalPricesAsync()
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

    return await dbContext.SP500Prices
                         .OrderBy(p => p.Date)
                         .ToListAsync();
  }
}

