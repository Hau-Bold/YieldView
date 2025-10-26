using YieldView.API.Models;

namespace YieldView.API.Services.Contract;

public interface IWilshireParser
{
  IEnumerable<WilshirePrice> Parse(string html);
}
