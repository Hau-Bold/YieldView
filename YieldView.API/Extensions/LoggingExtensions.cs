namespace YieldView.API.Extensions;

using global::YieldView.API.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Runtime.InteropServices;

public static class LoggingExtensions
{
  public static ILoggingBuilder AddPlatformLogging(this ILoggingBuilder logging, IConfiguration configuration)
  {
    logging.ClearProviders();
    logging.AddConsole();

    var logConfig = configuration.GetSection("Logging:File");
    string? path = logConfig.GetValue<string>("Path");

    // Set OS-specific default path if missing
    if (string.IsNullOrWhiteSpace(path))
    {
      path = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
          ? "/var/log/yieldview/Logs/YieldView.API"
          : @"C:\Users\user\Desktop\YieldView\Logs\YieldView.API";
    }

    // Ensure folder exists
    var folder = Path.GetDirectoryName(path) ?? path;
    Directory.CreateDirectory(folder);

    logging.AddFile(logConfig); // File logger will use the path from appsettings.json or default

    return logging;
  }
}

