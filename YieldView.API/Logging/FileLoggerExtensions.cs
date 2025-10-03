namespace YieldView.API.Logging;

public static class FileLoggerExtensions
{
  public static ILoggingBuilder AddFile(this ILoggingBuilder builder, IConfiguration configuration)
  {
    builder.AddProvider(new FileLoggerProvider(configuration));
    return builder;
  }
}
