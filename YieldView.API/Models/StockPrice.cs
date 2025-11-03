using System.ComponentModel.DataAnnotations;

namespace YieldView.API.Models;

public class StockPrice
{
  [Key]
  public DateTime Date { get; set; }

  public double Open { get; set; }

  public double High { get; set; }

  public double Low { get; set; }

  public double Close { get; set; }

  public long Volume { get; set; }

  public double AveragedClose { get; set; }

  public double GaussianAveragedClose { get; set; }

  public int PlateauIndex { get; set; }
}

public class BiduStockPrice : StockPrice { }

public class PlugStockPrice : StockPrice { }
public class PorscheAGStockPrice : StockPrice { }

public class AlibabaStockPrice : StockPrice { }
public class ConstellationBrandsStockPrice : StockPrice { }
public class KenvueStockPrice : StockPrice { }
public class LyondellBasellStockPrice : StockPrice { }
public class EastmanChemicalStockPrice : StockPrice { }
public class DowIncStockPrice : StockPrice { }
public class RheinmetallStockPrice : StockPrice { }
public class PfizerStockPrice : StockPrice { }
