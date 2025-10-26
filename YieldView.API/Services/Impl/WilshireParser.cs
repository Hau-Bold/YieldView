using System.Text.Json;
using YieldView.API.Models;
using YieldView.API.Services.Contract;

namespace YieldView.API.Services.Impl;

public class WilshireParser : IWilshireParser
{
  public IEnumerable<WilshirePrice> Parse(string jsonOrHtml)
  {
    var doc = JsonDocument.Parse(jsonOrHtml);
    var result = doc.RootElement.GetProperty("chart").GetProperty("result")[0];

    var timestamps = result.GetProperty("timestamp").EnumerateArray().ToArray();
    var quotes = result.GetProperty("indicators").GetProperty("quote")[0];
    var closes = quotes.GetProperty("close").EnumerateArray().ToArray();
    var opens = quotes.GetProperty("open").EnumerateArray().ToArray();
    var lows = quotes.GetProperty("low").EnumerateArray().ToArray();
    var highs = quotes.GetProperty("high").EnumerateArray().ToArray();

    var list = new List<WilshirePrice>();
    for (int i = 0; i < timestamps.Length; i++)
    {
      if (closes[i].ValueKind != JsonValueKind.Number)
      {
        continue;
      }

      var date = DateTimeOffset.FromUnixTimeSeconds(timestamps[i].GetInt64()).UtcDateTime;

      list.Add(new WilshirePrice
      {
        Date = date,
        Value = closes[i].GetDouble(),
        Open = opens[i].GetDouble(),
        High = highs[i].GetDouble(),
        Low = lows[i].GetDouble()
      });
    }

    return list;
  }
}
