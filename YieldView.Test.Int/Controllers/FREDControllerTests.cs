using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using YieldView.API.Data;
using YieldView.API.Models;
using YieldView.API.Test.sdk;

namespace YieldView.API.Test.Int.Controllers;

public class FREDControllerTests
{
  private CustomWebApplicationFactory factory;
  private HttpClient client;

  [SetUp]
  public void SetUp()
  {
    factory = new CustomWebApplicationFactory();
    client = factory.CreateClient();

    using var scope = factory.Services.CreateScope();


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

  [TearDown]
  public void TearDown()
  {
    client?.Dispose();
    factory?.Stopapplication();
    factory?.Dispose();
  }

  [Test]
  public async Task GetGDP_Returns_Ok_WithData()
  {
    // Arrange
    var from = new DateTime(2020, 1, 1).ToString("yyyy-MM-dd");
    var to = new DateTime(2022, 12, 31).ToString("yyyy-MM-dd");

    // Act
    var response = await client.GetAsync($"/api/fred/gdp?from={from}&to={to}");

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

    var result = await response.Content.ReadFromJsonAsync<List<GDPPrice>>();

    Assert.Multiple(() =>
    {
      Assert.That(result, Is.Not.Null);
      Assert.That(result, Has.Count.EqualTo(3));
      Assert.That(result![0].Date, Is.EqualTo(new DateTime(2020, 1, 1)));
    });
  }

  [Test]
  public async Task GetGDP_Returns_BadRequest_WhenMissingDates()
  {
    // Act
    var response = await client.GetAsync("/api/fred/gdp");

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    var msg = await response.Content.ReadAsStringAsync();
    Assert.That(msg, Does.Contain("Please provide both from and to dates"));
  }

  [Test]
  public async Task GetGDP_Returns_NotFound_WhenNoDataInRange()
  {
    // Arrange
    var from = new DateTime(2010, 1, 1).ToString("yyyy-MM-dd");
    var to = new DateTime(2011, 1, 1).ToString("yyyy-MM-dd");

    // Act
    var response = await client.GetAsync($"/api/fred/gdp?from={from}&to={to}");

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
  }

  [Test]
  public async Task GetW5000_Returns_Ok_WithData()
  {
    // Arrange
    var from = new DateTime(2020, 1, 1).ToString("yyyy-MM-dd");
    var to = new DateTime(2022, 12, 31).ToString("yyyy-MM-dd");

    // Act
    var response = await client.GetAsync($"/api/fred/w5000?from={from}&to={to}");
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

    var result = await response.Content.ReadFromJsonAsync<List<WilshirePrice>>();

    // Assert
    Assert.Multiple(() =>
    {
      Assert.That(result, Is.Not.Null);
      Assert.That(result, Has.Count.EqualTo(3));
      Assert.That(result![2].Value, Is.EqualTo(31000));
    });
  }

  [Test]
  public async Task GetW5000_Returns_NotFound_WhenNoData()
  {
    // Arrange
    var from = new DateTime(2030, 1, 1).ToString("yyyy-MM-dd");
    var to = new DateTime(2031, 1, 1).ToString("yyyy-MM-dd");

    // Act
    var response = await client.GetAsync($"/api/fred/w5000?from={from}&to={to}");

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
  }


  [Test]
  public async Task GetBuffettIndicator_Returns_Ok_AndCalculatesCorrectly()
  {
    // Arrange
    var from = new DateTime(2020, 1, 1).ToString("yyyy-MM-dd");
    var to = new DateTime(2022, 1, 1).ToString("yyyy-MM-dd");

    // Act
    var response = await client.GetAsync($"/api/fred/buffett-indicator?from={from}&to={to}");

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

    var result = await response.Content.ReadFromJsonAsync<List<BuffettIndicator>>();
    Assert.Multiple(() =>
    {
      Assert.That(result, Is.Not.Null);
      Assert.That(result, Has.Count.EqualTo(3));
    });

    var first = result![0];
    Assert.That(first.Value, Is.EqualTo((24000.0 / 20000.0) * 100.0).Within(0.1));
  }

  [Test]
  public async Task GetBuffettIndicator_Returns_NotFound_WhenNoData()
  {
    // Arrange
    var from = new DateTime(2010, 1, 1).ToString("yyyy-MM-dd");
    var to = new DateTime(2011, 1, 1).ToString("yyyy-MM-dd");

    // Act
    var response = await client.GetAsync($"/api/fred/buffett-indicator?from={from}&to={to}");

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
  }
}


