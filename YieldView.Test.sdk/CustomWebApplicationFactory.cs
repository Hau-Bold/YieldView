using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using YieldView.API.Data;

namespace YieldView.Test.sdk;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
  public CustomWebApplicationFactory() { }

  /// <summary>
  /// Workaround for ASP.Net core  bug https://github.com/dotnet/aspnetcore/issues/40271
  ///Host is stopped twice which causes iHostedservice.StopAsync to be called twice.
  /// </summary>
  internal void Stopapplication()
  {
    var hostLifetimeService = Services.GetRequiredService<IHostApplicationLifetime>();
    hostLifetimeService.StopApplication();
  }

  internal HttpClient StartApplication() => CreateClient();

  protected override IHost CreateHost(IHostBuilder builder)
  {

    var contentRoot = GetProjectPath("YieldView.API");
    builder.UseContentRoot(contentRoot);


    CopyDepsJson(contentRoot);


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
          options.UseInMemoryDatabase("TestDb"));
    });
  }

  private static void ConfigureAppSettings(IHostBuilder builder)
  {
    builder.ConfigureAppConfiguration(config =>
    {
      config.AddInMemoryCollection(new Dictionary<string, string?> { });
    });
  }

  private static void CopyDepsJson(string contentRoot)
  {
    var depsSource = Path.Combine(contentRoot, "bin", "Debug", "net8.0", "testhost.deps.json");
    var depsDest = Path.Combine(AppContext.BaseDirectory, "testhost.deps.json");

    if (!File.Exists(depsDest) && File.Exists(depsSource))
    {
      File.Copy(depsSource, depsDest, overwrite: true);
    }
  }

  internal new void Dispose() => base.Dispose();

  private static string GetProjectPath(string projectName)
  {
    var dir = Directory.GetCurrentDirectory();
    while (!string.IsNullOrEmpty(dir))
    {
      var potential = Path.Combine(dir, projectName);
      if (Directory.Exists(potential))
        return potential;

      dir = Directory.GetParent(dir)?.FullName ?? "";
    }

    throw new DirectoryNotFoundException($"Projektordner '{projectName}' konnte nicht gefunden werden.");
  }
}
