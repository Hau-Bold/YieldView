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

  public DbSet<AlibabaStockPrice> AlibabaPrices { get; set; }

  public DbSet<ConstellationBrandsStockPrice> ConstellationBrandsPrices { get; set; }

  public DbSet<KenvueStockPrice> KenvuePrices { get; set; }

  public DbSet<LyondellBasellStockPrice> LyondellBasellPrices { get; set; }

  public DbSet<EastmanChemicalStockPrice> EastmanChemicalPrices { get; set; }

  public DbSet<DowIncStockPrice> DowIncPrices { get; set; }
  public DbSet<RheinmetallStockPrice> RheinmetallPrices { get; set; }
}
