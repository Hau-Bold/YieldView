using System.Globalization;
using YieldView.API.Models;
using YieldView.API.Services.Contract;

namespace YieldView.API.Services.Impl;

public class GDPParser: IGDPParser
{
  public List<T> Parse<T>(string csv) where T : GDPPrice, new()
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
          double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
      {

        T res = new ()
        {
          Date = date,
          Value = value,
        };

        prices.Add(res);
      }
    }

    return prices;
  }

}
