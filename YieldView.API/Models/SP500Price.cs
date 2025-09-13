namespace YieldView.API.Models;

public class SP500Price
{
  public int Id { get; set; }
  public DateTime Date { get; set; }
  public double Close { get; set; }
  public bool IsLocalShadowPoint { get; set; } = false;
}
