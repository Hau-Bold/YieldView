using Microsoft.AspNetCore.Mvc;
using YieldView.API.Models;
using YieldView.API.Services.Impl.Providers;

namespace YieldView.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FREDController(FREDDataProvider dataProvider) : Controller
{
  [HttpGet("gdp")]
  public async Task<ActionResult<IEnumerable<GDPPrice>>> GetGDP([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (from == null || to == null)
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

  [HttpGet("w5000")]
  public async Task<ActionResult<IEnumerable<WilshirePrice>>> GetW5000([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (from == null || to == null)
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

  [HttpGet("buffett-indicator")]
  public async Task<ActionResult<IEnumerable<BuffettIndicator>>> GetBuffettIndicator(
    [FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (from == null || to == null)
      return BadRequest("Please provide both from and to dates.");

    var data = await dataProvider.GetBuffettIndicatorAsync(from.Value, to.Value);

    if (data.Count == 0)
      return NotFound("No Buffett Indicator data found for the given range.");

    return Ok(data);
  }

}
