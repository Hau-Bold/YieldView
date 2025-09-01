namespace YieldView.API.Configurations;

public class YieldCurveSource
{
  public required string BaseUrl { get; set; }
  public required List<int> Years { get; set; }
  public required FetchInterval FetchInterval { get; set; }
}

public class YieldCurveSourcesConfig : Dictionary<string, YieldCurveSource> { }

public enum FetchInterval
{
  Hourly,
  Every12Hours,
  Daily
}
