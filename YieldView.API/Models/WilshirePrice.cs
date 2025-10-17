using System.ComponentModel.DataAnnotations;

namespace YieldView.API.Models;

public class WilshirePrice 
{
  [Key]
  public DateTime Date { get; set; }

  public double Value { get; set; }

  public double Open { get; set; }

  public double High { get; set; }

  public double Low { get; set; }

}

