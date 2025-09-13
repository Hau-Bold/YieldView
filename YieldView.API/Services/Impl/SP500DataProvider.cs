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
    public async Task<List<SP500PriceWithVolatility>> GetHistoricalPricesWithVolatilityAsync(DateTime from, DateTime to, int dataInterval, double eps)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

        var prices = await dbContext.SP500Prices
            .Where(p => p.Date >= from && p.Date <= to)
            .OrderBy(p => p.Date)
            .ToListAsync();

    //MarkLocalMax(prices);
    MarkLokalShadowPoint(prices, eps);

        List<double> returns = GetReturns(prices);

        var result = new List<SP500PriceWithVolatility>();
        for (int i = 0; i < prices.Count; i++)
        {
            var volatility = CalculateVolatility(returns, i, dataInterval);

            result.Add(new SP500PriceWithVolatility
            {
                Date = prices[i].Date,
                Close = prices[i].Close,
                Volatility = volatility,
                IsLocalShadowPoint = prices[i].IsLocalShadowPoint
            });
        }

        return result;
    }

  public void MarkLocalMax(List<SP500Price> sp500Prices)
  {
    foreach (var p in sp500Prices)
    {
      p.IsLocalShadowPoint = false;
    }

    int i = 0;

    while( i < sp500Prices.Count -1)
    {
      int k = 1;
      var current = sp500Prices[i];

      // Move forward while the next close price is less than or equal to the current close price
      while (i + k < sp500Prices.Count && sp500Prices[i +k].Close   <= current.Close)
      {
        k++;
      }

      if(k > 1)
      {
        current.IsLocalShadowPoint = true;
        i += k;
      }
      else
      {
        i++;
      }

    }
  }

  public void MarkLokalShadowPoint(List<SP500Price> sp500Prices,double eps)
  {
    if(sp500Prices.Count <= 2)
    {
      return;
    }

    foreach (var p in sp500Prices)
    {
      p.IsLocalShadowPoint = false;
    }

    int i = 0;

    while (i < sp500Prices.Count - 1)
    {
      int k = 1;
      var current = sp500Prices[i];
      var initDiff = (current.Close - sp500Prices[i + 1].Close) / current.Close;


      // Move forward while 
      while (i + k < sp500Prices.Count)
      {
        var currentDiff = (current.Close - sp500Prices[i + k].Close) / current.Close;
        if (Math.Abs(currentDiff - initDiff) <= eps)
        {
          k++;
        }
        else
        {
          break;
        }
      }

      if (k > 1)
      {
        // mark interval
        for (int j = i; j < i + k; j++)
        {
          sp500Prices[i].IsLocalShadowPoint = true;
        }

        i += k;
      }
      else
      {
        i++;
      }
    }
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

