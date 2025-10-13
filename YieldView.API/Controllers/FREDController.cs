using Microsoft.AspNetCore.Mvc;
using YieldView.API.Models;
using YieldView.API.Services.Impl.Providers;

namespace YieldView.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FREDController(GDPDataProvider dataProvider) : Controller
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
      return NotFound("No prices found in the given date range.");
    }

    return Ok(prices);
  }
}
