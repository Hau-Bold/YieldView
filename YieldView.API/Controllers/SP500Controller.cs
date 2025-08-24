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
}
