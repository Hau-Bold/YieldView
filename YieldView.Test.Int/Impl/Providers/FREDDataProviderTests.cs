using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YieldView.API.Data;
using YieldView.API.Models;
using YieldView.API.Services.Impl.Providers;

namespace YieldView.API.Test.Int.Impl.Providers;

public class FREDDataProviderTests
{
  private ServiceProvider serviceProvider = null!;
  private IServiceScopeFactory scopeFactory = null!;
  private FREDDataProvider sut = null!;

  [SetUp]
  public void SetUp()
  {
    var services = new ServiceCollection();

    services.AddDbContext<YieldDbContext>(options =>
        options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

    serviceProvider = services.BuildServiceProvider();
    scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    sut = new FREDDataProvider(scopeFactory);

    SeedTestData();
  }

  [TearDown]
  public void TearDown() => serviceProvider.Dispose();

  #region GDP PRICE TESTS

  [Test]
  public async Task GetGDPPricesAsync_FiltersByDateRange()
  {
    var from = new DateTime(2020, 6, 1);
    var to = new DateTime(2022, 1, 1);

    var result = await sut.GetGDPPricesAsync(from, to);

    Assert.That(result, Has.Count.EqualTo(2));
    Assert.That(result.First().Date, Is.EqualTo(new DateTime(2021, 1, 1)));
  }

  #endregion

  #region W5000 PRICE TESTS

  [Test]
  public async Task GetW5000PricesAsync_ReturnsOrderedResults()
  {
    var from = new DateTime(2019, 1, 1);
    var to = new DateTime(2022, 12, 31);

    var result = await sut.GetW5000PricesAsync(from, to);

    Assert.That(result, Has.Count.EqualTo(3));
    Assert.That(result, Is.Ordered.By("Date"));
  }

  #endregion

  #region BUFFETT INDICATOR TESTS

  [Test]
  public async Task GetBuffettIndicatorAsync_CalculatesCorrectly()
  {
    var from = new DateTime(2020, 1, 1);
    var to = new DateTime(2022, 1, 1);

    var result = await sut.GetBuffettIndicatorAsync(from, to);

    Assert.That(result, Has.Count.EqualTo(3));

    var sample = result.First();
    var expected = (24000 / 20000.0) * 100.0;
    Assert.That(sample.Value, Is.EqualTo(expected).Within(0.01));
  }

  [Test]
  public async Task GetBuffettIndicatorAsync_SkipsEntries_WhenGDPMissing()
  {
    using (var scope = scopeFactory.CreateScope())
    {
      var db = scope.ServiceProvider.GetRequiredService<YieldDbContext>();
      db.GDPPrices.RemoveRange(db.GDPPrices);
      db.SaveChanges();
    }

    var from = new DateTime(2020, 1, 1);
    var to = new DateTime(2022, 1, 1);

    var result = await sut.GetBuffettIndicatorAsync(from, to);

    Assert.That(result, Is.Empty);
  }

  #endregion

  private void SeedTestData()
  {
    using var scope = scopeFactory.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

    db.GDPPrices.AddRange(
    [
          new GDPPrice { Date = new DateTime(2020, 1, 1), Value = 20000 },
          new GDPPrice { Date = new DateTime(2021, 1, 1), Value = 22000 },
          new GDPPrice { Date = new DateTime(2022, 1, 1), Value = 25000 },
      ]);

    db.WilshirePrices.AddRange(
    [
          new WilshirePrice { Date = new DateTime(2020, 1, 1), Value = 24000 },
          new WilshirePrice { Date = new DateTime(2021, 1, 1), Value = 27000 },
          new WilshirePrice { Date = new DateTime(2022, 1, 1), Value = 31000 },
      ]);

    db.SaveChanges();
  }
}
