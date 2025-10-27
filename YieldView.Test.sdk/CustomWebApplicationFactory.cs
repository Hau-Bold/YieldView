using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using YieldView.API.Data;
using YieldView.API.Services.Contract;

namespace YieldView.API.Test.sdk;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
  private readonly Mock<ISP500Service> sP500Service;
  private readonly Mock<IStockService> biduStockService;
  private readonly Mock<IWilshireService> wilshireService;
  private readonly Mock<IGrossDomesticProductService> grossDomesticProductService;
  private readonly Mock<ITreasuryXmlService> treasuryXmlService;

  public CustomWebApplicationFactory(Mock<ISP500Service>? sP500Service = null,
                                     Mock<IStockService>? biduStockService = null,
                                     Mock<IWilshireService>? wilshireService = null,
                                     Mock<IGrossDomesticProductService>? grossDomesticProductService = null,
                                     Mock<ITreasuryXmlService>? treasuryXmlService = null)
  {
    this.sP500Service = sP500Service ?? new Mock<ISP500Service>();
    this.biduStockService = biduStockService ?? new Mock<IStockService>();
    this.wilshireService = wilshireService ?? new Mock<IWilshireService>();
    this.grossDomesticProductService = grossDomesticProductService ?? new Mock<IGrossDomesticProductService>();
    this.treasuryXmlService = treasuryXmlService ?? new Mock<ITreasuryXmlService>();
  }

  private readonly string dbName = Guid.NewGuid().ToString();

  /// <summary>
  /// Workaround for ASP.Net core  bug https://github.com/dotnet/aspnetcore/issues/40271
  ///Host is stopped twice which causes iHostedservice.StopAsync to be called twice.
  /// </summary>
  internal void Stopapplication()
  {
    var hostLifetimeService = Services.GetRequiredService<IHostApplicationLifetime>();
    hostLifetimeService.StopApplication();
  }

  internal HttpClient StartApplication()
      => CreateClient();

  protected override IHost CreateHost(IHostBuilder builder)
  {
    ConfigureAppSettings(builder);
    ClearDatabase(builder);


    return base.CreateHost(builder);
  }

  private void ClearDatabase(IHostBuilder builder)
  {
    builder.ConfigureServices(services =>
    {
      var serviceprovider = services.BuildServiceProvider();
      using var scope = serviceprovider.CreateScope();
      var ctx = scope.ServiceProvider.GetRequiredService<YieldDbContext>();
      ctx.Database.EnsureDeleted();
      ctx.Database.EnsureCreated();
    });
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureTestServices(services =>
    {
      var descriptor = services.SingleOrDefault(
          d => d.ServiceType == typeof(DbContextOptions<YieldDbContext>));

      if (descriptor != null)
      {
        services.Remove(descriptor);
      }

      services.AddDbContext<YieldDbContext>(options =>
          options.UseInMemoryDatabase(dbName));

      var sp = services.BuildServiceProvider();
      using var scope = sp.CreateScope();
      var ctx = scope.ServiceProvider.GetRequiredService<YieldDbContext>();
      ctx.Database.EnsureDeleted();
      ctx.Database.EnsureCreated();

      services.AddSingleton(sP500Service.Object);
      services.AddSingleton(biduStockService.Object);
      services.AddSingleton(wilshireService.Object);
      services.AddSingleton(grossDomesticProductService.Object);
      services.AddSingleton(treasuryXmlService.Object);
    });
  }

  private static void ConfigureAppSettings(IHostBuilder builder)
  {
    builder.ConfigureAppConfiguration(config =>
    {
      config.AddInMemoryCollection(new Dictionary<string, string?>
      {

      });
    });
  }

  internal new void Dispose()
  {
    base.Dispose();
  }
}
