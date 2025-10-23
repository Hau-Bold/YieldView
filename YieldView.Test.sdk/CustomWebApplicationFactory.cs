using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YieldView.API.Data;

namespace YieldView.API.Test.sdk;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{

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
        services.Remove(descriptor);

      services.AddDbContext<YieldDbContext>(options =>
          options.UseInMemoryDatabase(dbName));

      var sp = services.BuildServiceProvider();
      using var scope = sp.CreateScope();
      var ctx = scope.ServiceProvider.GetRequiredService<YieldDbContext>();
      ctx.Database.EnsureDeleted();
      ctx.Database.EnsureCreated();
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
