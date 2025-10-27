using System.Globalization;
using YieldView.API.Models;
using YieldView.API.Services.Contract;

namespace YieldView.API.Services.Impl;

public class CSVStockParser : ICSVStockParser
{
  public List<T> Parse<T>(string csv) where T : StockPrice, new()
  {
    var lines = csv.Split("\n").Skip(1);

    List<T> prices = [];

    foreach (var line in lines)
    {
      if (string.IsNullOrWhiteSpace(line))
      {
        continue;
      }

      var parts = line.Split(',');

      if (DateTime.TryParse(parts[0], out var date) &&
          double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var open) &&
          double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var high) &&
          double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var low) &&
          double.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var close) &&
          long.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var volume)
          )
      {

        T res = new()
        {
          Date = date,
          Open = open,
          High = high,
          Low = low,
          Close = close,
          Volume = volume
        };

        prices.Add(res);
      }
    }

    return prices;
  }

}
