using YieldView.API.Models;

namespace YieldView.API.Services.Contract;

public interface IYieldSpreadProvider
{
    /// <summary>  
    /// Retrieves yield spreads for a specified country and time period.  
    /// </summary>  
    /// <param name="from">The start date of the time range.</param>  
    /// <param name="to">The end date of the time range.</param>  
    /// <param name="country">The country for which yield spreads are requested.</param>  
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of yield spreads.</returns>  
    Task<List<YieldSpread>> GetYieldSpreadsAsync(DateTime from, DateTime to, string country);
}
