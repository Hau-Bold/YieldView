using Microsoft.EntityFrameworkCore;
using YieldView.API.Data;
using YieldView.API.Models;

namespace YieldView.API.Services.Impl;

public class StockDataProvider(IServiceScopeFactory scopeFactory)
{
  public async Task<List<StockPrice>> GetBiduStockPricesAsync(DateTime from, DateTime to)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();
    var biduPrices = dbContext.BiduPrices.AsQueryable();

    return await GetStockPricesAsync(from, to, biduPrices);
  }

  public async Task<List<StockPrice>> GetPlugUSStockPricesAsync(DateTime from, DateTime to)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();
    var plugPrices = dbContext.PlugPrices.AsQueryable();

    return await GetStockPricesAsync(from, to, plugPrices);
  }


  private static async Task<List<StockPrice>> GetStockPricesAsync(DateTime from, DateTime to, IQueryable<StockPrice> dbStockPrices)
  {
    var stockData= await dbStockPrices.Where(p => p.Date >= from && p.Date <= to)
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
