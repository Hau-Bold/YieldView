using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YieldView.API.Data;
using YieldView.API.Models;
using YieldView.API.Services.Impl.Providers;

namespace YieldView.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class YieldCurveController(YieldDbContext context, YieldSpreadProvider yieldSpreadProvider) : ControllerBase
{
    [HttpGet("{country}/{date}")]
    public async Task<ActionResult<IEnumerable<YieldCurvePoint>>> GetYieldCurve(string country, DateTime date)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        var points = await context.USYieldCurvePoints
            .Where(p => p.Country.ToUpper() == country.ToUpper()
                        && p.Date >= startDate
                        && p.Date < endDate)
            .ToListAsync();

        if (points == null || points.Count == 0)
            return NotFound($"No data for {country} at {date:yyyy-MM-dd}.");

        return Ok(points);
    }

    [HttpGet("spread/{country}/{from}/{to}")]
    public async Task<ActionResult<IEnumerable<YieldSpread>>> GetYieldSpreads(
        string country, DateTime from, DateTime to)
    {
        try
        {
            var spreads = await yieldSpreadProvider.GetYieldSpreadsAsync(from, to, country);
            if (spreads == null || spreads.Count == 0)
            {
                return NotFound($"No spread data for {country} between {from:yyyy-MM-dd} and {to:yyyy-MM-dd}.");
            }

            return Ok(spreads);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
