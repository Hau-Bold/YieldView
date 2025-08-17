using Microsoft.AspNetCore.Mvc;
using YieldView.API.Models;
using YieldView.API.Services.Impl;

namespace YieldView.API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class SP500Controller : ControllerBase
  {
    private SP500DataProvider dataProvider;

    public SP500Controller(SP500DataProvider dataProvider)
    {
      this.dataProvider = dataProvider;
    }

    [HttpGet]
    public async Task<ActionResult<List<SP500Price>>> Get()
    {
      var prices = await dataProvider.GetHistoricalPricesAsync();
      return Ok(prices);
    }
  }

}
