using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YieldView.API.Data;
using YieldView.API.Models;

namespace YieldView.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class YieldCurveController : ControllerBase
  {
      private readonly YieldDbContext _context;

      public YieldCurveController(YieldDbContext context)
      {
          _context = context;
      }

  [HttpGet("{country}/{date}")]
  public async Task<ActionResult<IEnumerable<YieldCurvePoint>>> GetYieldCurve(string country, DateTime date)
  {

    var startDate = date.Date; 
    var endDate = startDate.AddDays(1); 

    var points = await _context.USYieldCurvePoints
        .Where(p => p.Country.ToUpper() == country.ToUpper()
                    && p.Date >= startDate
                    && p.Date < endDate)
        .ToListAsync();

    if (points == null || points.Count == 0)
      return NotFound($"No data for {country} at {date:yyyy-MM-dd}.");

    return Ok(points);
  }
}
