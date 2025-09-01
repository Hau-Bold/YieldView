using Microsoft.EntityFrameworkCore;
using YieldView.API.Data;
using YieldView.API.Models;

namespace YieldView.API.Services.Impl;

public class YieldSpreadProvider(IServiceScopeFactory scopeFactory)
{
  public async Task<List<YieldSpread>> GetYieldSpreadsAsync(DateTime from, DateTime to, string country)
  {
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<YieldDbContext>();
    IQueryable<YieldCurvePoint> query = country.ToUpper() switch
    {
      "US" => dbContext.USYieldCurvePoints,
      _ => throw new ArgumentException($"Unsupported country: {country}"),
    };
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

  /// <summary>
  /// Votality
  /// </summary>
  public async Task<List<SP500PriceWithVolatility>> GetHistoricalPricesWithVolatilityAsync(
      DateTime from, DateTime to, int dataInterval = 20)
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
      double? volatility = null;

      // here: this should be optimized: just take everything before until windowsize
      if (i >= dataInterval)
      {
        var windowReturns = returns.Skip(i - dataInterval).Take(dataInterval).ToList();
        double mean = windowReturns.Average();
        double variance = windowReturns.Average(r => Math.Pow(r - mean, 2));
        volatility = Math.Sqrt(variance);
      }

      result.Add(new SP500PriceWithVolatility
      {
        Date = prices[i].Date,
        Close = prices[i].Close,
        Volatility = volatility
      });
    }

    return result;
  }

  // how are returns computed? Really a log?
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

  //embedd there the other model
  public class SP500PriceWithVolatility
  {
    public DateTime Date { get; set; }
    public double Close { get; set; }
    public double? Volatility { get; set; }
  }

}
