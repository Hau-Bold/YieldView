using Microsoft.AspNetCore.Mvc;
using YieldView.API.Models;
using YieldView.API.Services.Impl;

namespace YieldView.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SP500Controller(SP500DataProvider dataProvider) : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IEnumerable<SP500Price>>> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (from == null || to == null)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var prices = await dataProvider.GetHistoricalPricesAsync(from.Value, to.Value);

    if (!prices.Any())
    {
      return NotFound("No prices found in the given date range.");
    }

    return Ok(prices);
  }


  //GET https://localhost:7031/api/sp500/volatility?from=2024-08-01&to=2025-08-01&dataInterval=20
  [HttpGet("volatility")]
  public async Task<ActionResult<IEnumerable<SP500PriceWithVolatility>>>
    GetHistoricalPricesWithVolatility(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int dataInterval,
        [FromQuery] double eps)
  {
    try
    {
      var results = await dataProvider.GetHistoricalPricesWithVolatilityAsync(from, to, dataInterval,eps);

      if (results == null || results.Count == 0)
      {
        return NotFound($"No volatility data between {from:yyyy-MM-dd} and {to:yyyy-MM-dd}.");
      }

      return Ok(results);
    }
    catch (ArgumentException ex)
    {
      return BadRequest(ex.Message);
    }
  }

}
