using YieldView.API.Models;

namespace YieldView.API.Services.Contract;

public interface IGDPParser
{
  List<T> Parse<T>(string csv) where T: GDPPrice, new();
}
