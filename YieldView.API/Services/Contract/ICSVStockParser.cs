using YieldView.API.Models;

namespace YieldView.API.Services.Contract;

public interface ICSVStockParser
{
  List<T> Parse<T>(string csv) where T: StockPrice, new();
}
