using Microsoft.EntityFrameworkCore;
using YieldView.API.Models;

namespace YieldView.API.Data
{
    public class YieldDbContext: DbContext
    {
        public YieldDbContext(DbContextOptions<YieldDbContext> options) : base(options) { }

        public DbSet<YieldCurvePoint> YieldCurvePoints { get; set; }
    }
}
