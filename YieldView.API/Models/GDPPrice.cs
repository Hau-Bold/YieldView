using System.ComponentModel.DataAnnotations;

namespace YieldView.API.Models;

public class GDPPrice
{
  [Key]
  public DateTime Date { get; set; }

  public double Value { get; set; }
}
