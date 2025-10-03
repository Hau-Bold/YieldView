namespace YieldView.API.Services.Impl;

public class PlateauDetector
{




}


// a plateau has a start and an end, a reference value and also a numerical bound 
public class Plateau
{
  public DateTime StartDate { get; set; }
  public DateTime EndDate { get; set; }
  public double ReferenceValue { get; set; }
  public double Bound { get; set; }
}
