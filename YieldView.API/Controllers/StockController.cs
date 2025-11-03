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

  [HttpGet("alibaba")]
  public async Task<ActionResult<IEnumerable<StockPrice>>> GetAlibaba([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (AreInputDatesValid(from, to) == false)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var prices = await dataProvider.GetAlibabaStockPricesAsync(from.Value, to.Value);

    if (prices.Count == 0)
    {
      return NotFound("No prices found in the given date range.");
    }

    return Ok(prices);
  }

  [HttpGet("constellationbrands")]
  public async Task<ActionResult<IEnumerable<StockPrice>>> GetConstellationbrands([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (AreInputDatesValid(from, to) == false)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var prices = await dataProvider.GetConstellationBrandsPricesAsync(from.Value, to.Value);

    if (prices.Count == 0)
    {
      return NotFound("No prices found in the given date range.");
    }

    return Ok(prices);
  }

  [HttpGet("kenvue")]
  public async Task<ActionResult<IEnumerable<StockPrice>>> GetKenvue([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (AreInputDatesValid(from, to) == false)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var prices = await dataProvider.GetKenvuePricesAsync(from.Value, to.Value);

    if (prices.Count == 0)
    {
      return NotFound("No prices found in the given date range.");
    }

    return Ok(prices);
  }

  [HttpGet("lyondellbasell")]
  public async Task<ActionResult<IEnumerable<StockPrice>>> GetLyondellBasell([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (AreInputDatesValid(from, to) == false)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var prices = await dataProvider.GetLyondellBasellPricesAsync(from.Value, to.Value);

    if (prices.Count == 0)
    {
      return NotFound("No prices found in the given date range.");
    }

    return Ok(prices);
  }

  [HttpGet("eastmanchemical")]
  public async Task<ActionResult<IEnumerable<StockPrice>>> GetEastmanchemical([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (AreInputDatesValid(from, to) == false)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var prices = await dataProvider.GetEastmanChemicalPricesAsync(from.Value, to.Value);

    if (prices.Count == 0)
    {
      return NotFound("No prices found in the given date range.");
    }

    return Ok(prices);
  }

  [HttpGet("dowinc")]
  public async Task<ActionResult<IEnumerable<StockPrice>>> GetDowInc([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (AreInputDatesValid(from, to) == false)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var prices = await dataProvider.GetDowIncPricesAsync(from.Value, to.Value);

    if (prices.Count == 0)
    {
      return NotFound("No prices found in the given date range.");
    }

    return Ok(prices);
  }

  [HttpGet("rheinmetall")]
  public async Task<ActionResult<IEnumerable<StockPrice>>> GetRheinmetall([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (AreInputDatesValid(from, to) == false)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var prices = await dataProvider.GetRheinmetallPricesAsync(from.Value, to.Value);

    if (prices.Count == 0)
    {
      return NotFound("No prices found in the given date range.");
    }

    return Ok(prices);
  }

  [HttpGet("pfizer")]
  public async Task<ActionResult<IEnumerable<StockPrice>>> GetPfizer([FromQuery] DateTime? from, [FromQuery] DateTime? to)
  {
    if (AreInputDatesValid(from, to) == false)
    {
      return BadRequest("Please provide both from and to dates.");
    }

    var prices = await dataProvider.GetPfizerPricesAsync(from.Value, to.Value);

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
