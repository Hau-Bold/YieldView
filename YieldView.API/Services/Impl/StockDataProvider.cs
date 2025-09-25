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

    var stockData= await dbSet.Where(p => p.Date >= from && p.Date <= to)
                      .OrderBy(p => p.Date)
                      .ToListAsync();

    // Todo: Maybe wrap in extension
    double averageClose = 0;
    int plateauIndex = 0;
    int localGaussian = 0;
    for (int i=0;i< stockData.Count;i++)
    {
      var current = stockData[i];
      
      averageClose += current.Close;
      current.AveragedClose = averageClose / (i + 1);
      var currentGaussian = (int)Math.Round(current.AveragedClose);

      if(localGaussian != currentGaussian)
      {
        localGaussian = currentGaussian;
        plateauIndex++;
      }

      current.GaussianAveragedClose = currentGaussian;
      current.PlateauIndex = plateauIndex;
    }

    return stockData;
  }
}
