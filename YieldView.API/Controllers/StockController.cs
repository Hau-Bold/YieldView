using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using YieldView.API.Models;
using YieldView.API.Services.Impl.Providers;

namespace YieldView.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StockController(StockDataProvider dataProvider) : Controller
{
  [HttpGet("bidu")]
  public async Task<ActionResult<IEnumerable<StockPrice>>> GetBidu([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (AreInputDatesValid(from, to) == false)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var prices = await dataProvider.GetBiduStockPricesAsync(from.Value, to.Value);

    if (prices.Count == 0)
    {
      return NotFound("No prices found in the given date range.");
    }

    return Ok(prices);
  }

  [HttpGet("plug.us")]
  public async Task<ActionResult<IEnumerable<StockPrice>>> GetPlug([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (AreInputDatesValid(from, to) == false)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var prices = await dataProvider.GetPlugUSStockPricesAsync(from.Value, to.Value);

    if (prices.Count == 0)
    {
      return NotFound("No prices found in the given date range.");
    }

    return Ok(prices);
  }

  [HttpGet("porscheag")]
  public async Task<ActionResult<IEnumerable<StockPrice>>> GetPorscheAg([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (AreInputDatesValid(from, to) == false)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var prices = await dataProvider.GetPorscheAGStockPricesAsync(from.Value, to.Value);

    if (prices.Count == 0)
    {
      return NotFound("No prices found in the given date range.");
    }

    return Ok(prices);
  }

  private static bool AreInputDatesValid([NotNullWhen(true)] DateTime? from, [NotNullWhen(true)] DateTime? to)
  {
    return from.HasValue && to.HasValue && from.Value.CompareTo(to.Value) < 0;
  }
}
