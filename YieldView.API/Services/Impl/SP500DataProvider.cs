using Microsoft.EntityFrameworkCore;
using YieldView.API.Data;
using YieldView.API.Models;
using YieldView.API.Services.Contract;

namespace YieldView.API.Services.Impl;

public class SP500DataProvider(IServiceScopeFactory scopeFactory): ISP500DataProvider
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

    /// <inheritdoc />  
    public async Task<List<SP500PriceWithVolatility>> GetHistoricalPricesWithVolatilityAsync(DateTime from, DateTime to, int dataInterval)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

        var prices = await dbContext.SP500Prices
            .Where(p => p.Date >= from && p.Date <= to)
            .OrderBy(p => p.Date)
            .ToListAsync();

        List<double> returns = GetReturns(prices);

        var result = new List<SP500PriceWithVolatility>();
        for (int i = 0; i < prices.Count; i++)
        {
            var volatility = CalculateVolatility(returns, i, dataInterval);

            result.Add(new SP500PriceWithVolatility
            {
                Date = prices[i].Date,
                Close = prices[i].Close,
                Volatility = volatility
            });
        }

        return result;
    }



  private static double? CalculateVolatility(List<double> returns, int currentIndex, int dataInterval)
  {
    if (currentIndex < dataInterval)
      return null;

    var windowReturns = returns.Skip(currentIndex - dataInterval).Take(dataInterval).ToList();
    double mean = windowReturns.Average();
    double variance = windowReturns.Average(r => Math.Pow(r - mean, 2));
    return Math.Sqrt(variance);
  }

  private static List<double> GetReturns(List<SP500Price> prices)
  {
    var returns = new List<double>();
    for (int i = 1; i < prices.Count; i++)
    {
      double ret = Math.Log(prices[i].Close / prices[i - 1].Close);
      returns.Add(ret);
    }

    return returns;
  }
}

