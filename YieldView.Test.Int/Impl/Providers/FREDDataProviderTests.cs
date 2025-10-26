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

  private readonly List<WilshirePrice> wilshirePrices =
  [
      new() { Date = new DateTime(2020, 1, 1), Value = 24000 },
      new() { Date = new DateTime(2021, 1, 1), Value = 27000 },
      new() { Date = new DateTime(2022, 1, 1), Value = 31000 }
  ];

  private readonly List<GDPPrice> gdpPrices =
  [
      new() { Date = new DateTime(2020, 1, 1), Value = 20000 },
      new() { Date = new DateTime(2021, 1, 1), Value = 22000 },
      new() { Date = new DateTime(2022, 1, 1), Value = 25000 }
  ];

  [SetUp]
  public void SetUp()
  {
    var services = new ServiceCollection();

    services.AddDbContext<YieldDbContext>(options =>
        options.UseInMemoryDatabase("testdb"));

    serviceProvider = services.BuildServiceProvider();
    scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    sut = new FREDDataProvider(scopeFactory);
  }

  [TearDown]
  public void TearDown() => serviceProvider.Dispose();

  #region GDP PRICE TESTS

  [Test]
  public async Task GetGDPPricesAsync_FiltersByDateRange()
  {
    // Arrange
    SetUpContext(gdpPrices: gdpPrices);
    var from = new DateTime(2020, 6, 1);
    var to = new DateTime(2022, 1, 1);

    // Act
    var result = await sut.GetGDPPricesAsync(from, to);

    // Assert
    Assert.That(result, Has.Count.EqualTo(2));
    Assert.That(result.First().Date, Is.EqualTo(new DateTime(2021, 1, 1)));
  }

  #endregion

  #region W5000 PRICE TESTS

  [Test]
  public async Task GetW5000PricesAsync_ReturnsOrderedResults()
  {
    // Arrange
    SetUpContext(wilshirePrices: wilshirePrices);
    var from = new DateTime(2019, 1, 1);
    var to = new DateTime(2022, 12, 31);

    // Act
    var result = await sut.GetW5000PricesAsync(from, to);

    // Assert
    Assert.That(result, Has.Count.EqualTo(3));
    Assert.That(result, Is.Ordered.By("Date"));
  }

  #endregion

  #region BUFFETT INDICATOR TESTS

  [Test]
  public async Task GetBuffettIndicatorAsync_CalculatesCorrectly()
  {
    // Arrange
    SetUpContext(gdpPrices: gdpPrices, wilshirePrices: wilshirePrices);
    var from = new DateTime(2020, 1, 1);
    var to = new DateTime(2022, 1, 1);

    // Act
    var result = await sut.GetBuffettIndicatorAsync(from, to);

    // Assert
    Assert.That(result, Has.Count.EqualTo(3));

    var sample = result.First();
    var expected = (24000 / 20000.0) * 100.0;
    Assert.That(sample.Value, Is.EqualTo(expected).Within(0.01));
  }

  [Test]
  public async Task GetBuffettIndicatorAsync_SkipsEntries_WhenGDPMissing()
  {
    // Arrange
    SetUpContext(wilshirePrices: wilshirePrices);
    var from = new DateTime(2020, 1, 1);
    var to = new DateTime(2022, 1, 1);

    // Act
    var result = await sut.GetBuffettIndicatorAsync(from, to);

    // Assert
    Assert.That(result, Is.Empty);
  }

  #endregion

  #region HELPER

  private void SetUpContext(
      List<GDPPrice>? gdpPrices = null,
      List<WilshirePrice>? wilshirePrices = null)
  {
    using var scope = scopeFactory.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<YieldDbContext>();

    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();

    if (gdpPrices is { Count: > 0 })
    {
      db.GDPPrices.AddRange(gdpPrices);
    }

    if (wilshirePrices is { Count: > 0 })
    {
      db.WilshirePrices.AddRange(wilshirePrices);
    }

    db.SaveChanges();
  }

  #endregion
}
