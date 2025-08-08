using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using YieldView.API.Configurations;
using YieldView.API.Data;
using YieldView.API.Models;
using YieldView.API.Services.Contract;

namespace YieldView.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class YieldCurveController : ControllerBase
    {
        private readonly YieldDbContext _context;
        private readonly ITreasuryXmlService _treasuryXmlService;

        public YieldCurveController(YieldDbContext context,ITreasuryXmlService treasuryCsvService)
        {
            _context = context;
            _treasuryXmlService = treasuryCsvService;
        }

        [HttpGet("{country}/{year}")]
        public async Task<ActionResult<IEnumerable<YieldCurvePoint>>> GetYieldCurve(string country, int year)
        {
            var points = await _treasuryXmlService.DownloadAndParseYieldCurveAsync(country,year);
            return Ok(points);
        }
    }
}
