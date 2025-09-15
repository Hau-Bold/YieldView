namespace YieldView.API.Models;

public record SP500PriceWithVolatility(DateTime Date, double Close, double? Volatility);
