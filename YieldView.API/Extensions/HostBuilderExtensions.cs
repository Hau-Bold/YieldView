namespace YieldView.API.Extensions;

using System.Runtime.InteropServices;


public static class HostBuilderExtensions
{
  public static WebApplicationBuilder UseOsBasedEnvironment(this WebApplicationBuilder builder)
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
      builder.Environment.EnvironmentName = "Linux";
      builder.Configuration.Sources.Clear();
      builder.Configuration.AddJsonFile("appsettings.Linux.json", optional: false, reloadOnChange: true);
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      builder.Environment.EnvironmentName = "Windows";
      builder.Configuration.Sources.Clear();
      builder.Configuration.AddJsonFile("appsettings.Windows.json", optional: true, reloadOnChange: true);
    }
    else
    {
      throw new PlatformNotSupportedException(
          $"YieldView.API does not support this operating system: {RuntimeInformation.OSDescription}");
    }


    return builder;
  }
}

