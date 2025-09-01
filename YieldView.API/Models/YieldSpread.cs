namespace YieldView.API.Models;

public class YieldSpread
{
  public DateTime Date { get; set; }
  public double? TenYear { get; set; }
  public double? SixMonth { get; set; }
  public double? Spread { get; set; } 
}
