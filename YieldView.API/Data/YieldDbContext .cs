using Microsoft.EntityFrameworkCore;
using YieldView.API.Models;

namespace YieldView.API.Data;

public class YieldDbContext: DbContext
{
      public YieldDbContext(DbContextOptions<YieldDbContext> options) : base(options) { }

      public DbSet<YieldCurvePoint> USYieldCurvePoints { get; set; }

      public DbSet<YieldCurvePoint> DEYieldCurvePoints { get; set; }

      public DbSet<SP500Price> SP500Prices { get; set; }
}
