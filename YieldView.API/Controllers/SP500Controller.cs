using Microsoft.AspNetCore.Mvc;
using YieldView.API.Models;
using YieldView.API.Services.Impl.Providers;

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

        if (prices.Count==0)
        {
            return NotFound("No prices found in the given date range.");
        }

        return Ok(prices);
    }

    [HttpGet("volatility")]
    public async Task<ActionResult<IEnumerable<SP500PriceWithVolatility>>> GetHistoricalPricesWithVolatility(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int dataInterval)
    {
        try
        {
            var results = await dataProvider.GetHistoricalPricesWithVolatilityAsync(from, to, dataInterval);

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
