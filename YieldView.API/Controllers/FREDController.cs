using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using YieldView.API.Models;
using YieldView.API.Services.Impl.Providers;

namespace YieldView.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FREDController(FREDDataProvider dataProvider) : Controller
{

  /// <summary>
  /// Retrieves Wilshire5000 data for the specified date range.
  /// </summary>
  /// <param name="from">
  /// The start date of the requested time range.  
  /// </param>
  /// <param name="to">
  /// The end date of the requested time range.  
  /// </param>
  /// <returns>
  /// An <see cref="ActionResult{T}"/> containing a collection of 
  /// <see cref="GDPPrice"/> objects that fall within the specified date range.  
  /// Returns <see cref="BadRequestObjectResult"/> if the input dates are invalid or missing,  
  /// and <see cref="NotFoundObjectResult"/> if no data is available for the given range.
  /// </returns>
  /// <remarks>
  /// The <c>from</c> and <c>to</c> parameters are nullable to allow flexible querying.
  /// If either is omitted, the request is considered invalid.
  /// </remarks>
  [HttpGet("gdp")]
  public async Task<ActionResult<IEnumerable<GDPPrice>>> GetGDP([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (AreInputDatesValid(from, to) == false)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var prices = await dataProvider.GetGDPPricesAsync(from.Value, to.Value);

    if (prices.Count == 0)
    {
      return NotFound("No gdp prices found in the given date range.");
    }

    return Ok(prices);
  }



  /// <summary>
  /// Retrieves Wilshire5000 data for the specified date range.
  /// </summary>
  /// <param name="from">
  /// The start date of the requested time range.  
  /// </param>
  /// <param name="to">
  /// The end date of the requested time range.  
  /// </param>
  /// <returns>
  /// An <see cref="ActionResult{T}"/> containing a collection of 
  /// <see cref="WilshirePrice"/> objects that fall within the specified date range.  
  /// Returns <see cref="BadRequestObjectResult"/> if the input dates are invalid or missing,  
  /// and <see cref="NotFoundObjectResult"/> if no data is available for the given range.
  /// </returns>
  /// <remarks>
  /// The <c>from</c> and <c>to</c> parameters are nullable to allow flexible querying.
  /// If either is omitted, the request is considered invalid.
  /// </remarks>
  [HttpGet("w5000")]
  public async Task<ActionResult<IEnumerable<WilshirePrice>>> GetW5000([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (AreInputDatesValid(from, to) == false)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var prices = await dataProvider.GetW5000PricesAsync(from.Value, to.Value);

    if (prices.Count == 0)
    {
      return NotFound("No w5000 prices found in the given date range.");
    }

    return Ok(prices);
  }

  /// <summary>
  /// Retrieves Buffett Indicator data for the specified date range.
  /// </summary>
  /// <param name="from">
  /// The start date of the requested time range.  
  /// </param>
  /// <param name="to">
  /// The end date of the requested time range.  
  /// </param>
  /// <returns>
  /// An <see cref="ActionResult{T}"/> containing a collection of 
  /// <see cref="BuffettIndicator"/> objects that fall within the specified date range.  
  /// Returns <see cref="BadRequestObjectResult"/> if the input dates are invalid or missing,  
  /// and <see cref="NotFoundObjectResult"/> if no data is available for the given range.
  /// </returns>
  /// <remarks>
  /// The <c>from</c> and <c>to</c> parameters are nullable to allow flexible querying.
  /// If either is omitted, the request is considered invalid.
  /// </remarks>
  [HttpGet("buffett-indicator")]
  public async Task<ActionResult<IEnumerable<BuffettIndicator>>> GetBuffettIndicator(
    [FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (AreInputDatesValid(from, to) == false)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var data = await dataProvider.GetBuffettIndicatorAsync(from.Value, to.Value);

    if (data.Count == 0)
    {
      return NotFound("No Buffett Indicator data found for the given range.");
    }

    return Ok(data);
  }

  private static bool AreInputDatesValid([NotNullWhen(true)] DateTime? from, [NotNullWhen(true)] DateTime? to)
  {
    return from.HasValue && to.HasValue && from.Value.CompareTo(to.Value) < 0;
  }
}
