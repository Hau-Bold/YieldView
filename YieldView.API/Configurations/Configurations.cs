namespace YieldView.API.Configurations
{
  public class YieldCurveSource
  {
    public string BaseUrl { get; set; }
    public List<int> Years { get; set; }
  }

  public class YieldCurveSourcesConfig : Dictionary<string, YieldCurveSource> { }
}
