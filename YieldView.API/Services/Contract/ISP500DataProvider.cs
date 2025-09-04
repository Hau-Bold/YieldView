using YieldView.API.Models;

namespace YieldView.API.Services.Contract;

public interface ISP500DataProvider
{
  /// <summary>  
  /// Retrieves historical prices of the S&P 500 index along with their volatility  
  /// for a specified time range and data interval.  
  /// </summary>  
  /// <param name="from">The start date of the time range.</param>  
  /// <param name="to">The end date of the time range.</param>  
  /// <param name="dataInterval">The interval in days for the data points.</param>  
  /// <returns>A task that represents the asynchronous operation. The task result contains a list of S&P 500 prices with their associated volatility.</returns>  
  Task<List<SP500PriceWithVolatility>> GetHistoricalPricesWithVolatilityAsync(
          DateTime from, DateTime to, int dataInterval = 20);
}
