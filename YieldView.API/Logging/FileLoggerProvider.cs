namespace YieldView.API.Logging;

public class FileLoggerProvider : ILoggerProvider
{
  private readonly string filePath;
  private readonly bool append;
  private readonly long maxFileSize;
  private readonly int maxRollingFiles;

  public FileLoggerProvider(IConfiguration configuration)
  {
    this.filePath = configuration["Path"] ?? "app.log";
    this.append = bool.TryParse(configuration["Append"], out var append) && append;
    this.maxFileSize = long.TryParse(configuration["MaxFileSize"], out var size) ? size : 10_000_000;
    this.maxRollingFiles = int.TryParse(configuration["MaxRollingFiles"], out var files) ? files : 5;
  }

  public ILogger CreateLogger(string categoryName) =>
         new FileLogger(categoryName, filePath, append, maxFileSize, maxRollingFiles);
  public void Dispose() { }
}
