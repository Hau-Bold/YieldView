namespace YieldView.API.Configurations;

internal class DataFetchHelper
{
  internal static TimeSpan GetDelayForInterval(FetchInterval interval) =>
       interval switch
       {
         FetchInterval.Hourly => TimeSpan.FromHours(1),
         FetchInterval.Every12Hours => TimeSpan.FromHours(12),
         FetchInterval.Daily => TimeSpan.FromDays(1),
         _ => TimeSpan.FromDays(1)
       };
}
