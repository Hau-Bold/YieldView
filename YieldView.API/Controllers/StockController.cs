using Microsoft.AspNetCore.Mvc;
using YieldView.API.Models;
using YieldView.API.Services.Impl;

namespace YieldView.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StockController(StockDataProvider dataProvider) : Controller
{
  //http://localhost:5000/api/stock/bidu?from=2021-01-01&to=2025-01-01
  [HttpGet("bidu")]
  public async Task<ActionResult<IEnumerable<BiduStockPrice>>> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (from == null || to == null)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var prices = await dataProvider.GetStockPricesAsync<BiduStockPrice>(from.Value, to.Value);

    if (prices.Count == 0)
    {
      return NotFound("No prices found in the given date range.");
    }

    return Ok(prices);
  }
}
