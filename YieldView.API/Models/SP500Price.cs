namespace YieldView.API.Models;

public class SP500Price
{
  public int Id { get; set; }
  public DateTime Date { get; set; }
  public decimal Close { get; set; }
}
