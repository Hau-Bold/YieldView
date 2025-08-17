namespace YieldView.API.Configurations;

public class YieldCurveSource
{
  public required string BaseUrl { get; set; }
  public required List<int> Years { get; set; }
}

public class YieldCurveSourcesConfig : Dictionary<string, YieldCurveSource> { }


