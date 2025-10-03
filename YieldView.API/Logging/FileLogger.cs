namespace YieldView.API.Logging;

using Microsoft.Extensions.Logging;
using System;
using System.IO;

public class FileLogger : ILogger
{
  private readonly string filePath;
  private readonly string categoryName;
  private readonly bool append;
  private readonly long maxFileSize;
  private readonly int maxRollingFiles;
  private static readonly object _lock = new();

  public FileLogger(string categoryName, string filePath, bool append, long maxFileSize, int maxRollingFiles)
  {
    this.categoryName = categoryName;
    this.filePath = filePath;
    this.append = append;
    this.maxFileSize = maxFileSize;
    this.maxRollingFiles = maxRollingFiles;

    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
  }

  public IDisposable BeginScope<TState>(TState state) => null!;
  public bool IsEnabled(LogLevel logLevel) => true;

  public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
  {
    if (!IsEnabled(logLevel))
    {
      return;
    }

    var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {categoryName}: {formatter(state, exception)}{Environment.NewLine}";

    lock (_lock)
    {
      RollIfNeeded();
      File.AppendAllText(filePath, message);
    }
  }

  private void RollIfNeeded()
  {
    if (!File.Exists(filePath))
    {
      return;
    }

    var fileInfo = new FileInfo(filePath);
    if (fileInfo.Length < maxFileSize)
    {
      return;
    }

    for (int i = maxRollingFiles - 1; i >= 1; i--)
    {
      var src = $"{filePath}.{i}";
      var dest = $"{filePath}.{i + 1}";
      if (File.Exists(src))
      {
        if (File.Exists(dest))
        {
          File.Delete(dest);
        }
        File.Move(src, dest);
      }
    }

    var rolled = $"{filePath}.1";
    if (File.Exists(rolled))
    {
      File.Delete(rolled);
    }
    File.Move(filePath, rolled);
  }
}

