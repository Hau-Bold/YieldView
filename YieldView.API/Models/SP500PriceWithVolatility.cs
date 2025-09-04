namespace YieldView.API.Models;

public class SP500PriceWithVolatility
{
  public DateTime Date { get; set; }
  public double Close { get; set; }
  public double? Volatility { get; set; }
}
