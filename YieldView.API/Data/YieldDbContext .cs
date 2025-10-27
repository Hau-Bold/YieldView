using Microsoft.EntityFrameworkCore;
using YieldView.API.Models;

namespace YieldView.API.Data;

public class YieldDbContext(DbContextOptions<YieldDbContext> options) : DbContext(options)
{
  public DbSet<YieldCurvePoint> USYieldCurvePoints { get; set; }

  public DbSet<YieldCurvePoint> DEYieldCurvePoints { get; set; }

  public DbSet<SP500Price> SP500Prices { get; set; }

  public DbSet<BiduStockPrice> BiduPrices { get; set; }

  public DbSet<PlugStockPrice> PlugPrices { get; set; }

  public DbSet<PorscheAGStockPrice> PorscheAGPrices { get; set; }

  public DbSet<GDPPrice> GDPPrices { get; set; }

  public DbSet<WilshirePrice> WilshirePrices { get; set; }
}
