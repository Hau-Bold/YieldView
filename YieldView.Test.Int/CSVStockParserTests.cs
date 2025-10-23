namespace YieldView.API.Test.Int;

using System;
using YieldView.API.Models;
using YieldView.API.Services.Impl;

[TestFixture(typeof(BiduStockPrice))]
[TestFixture(typeof(PlugStockPrice))]
public class CSVStockParserTests<T> where T : StockPrice, new()
{

  private readonly CSVStockParser sut = new();

  [Test]
  public void Parse_ValidCsv_ReturnsParsedList()
  {
    // Arrange
    string csv = "Date,Open,High,Low,Close,Volume\n" +
                 "2023-01-01,100.5,110.2,99.8,105.3,1000000\n" +
                 "2023-01-02,106.0,112.1,104.7,110.9,2000000";

    // Act
    var result = sut.Parse<T>(csv);

    // Assert
    Assert.Multiple(() =>
    {
      Assert.That(result, Has.Count.EqualTo(2));
      Assert.That(new DateTime(2023, 1, 1), Is.EqualTo(result[0].Date));
      Assert.That(result[0].Open, Is.EqualTo(100.5));
      Assert.That(result[0].High, Is.EqualTo(110.2));
      Assert.That(result[0].Low, Is.EqualTo(99.8));
      Assert.That(result[0].Close, Is.EqualTo(105.3));
      Assert.That(result[0].Volume, Is.EqualTo(1000000));
    }
    );
  }

  [Test]
  public void Parse_EmptyCsv_ReturnsEmptyList()
  {
    // Arrange
    string csv = "Date,Open,High,Low,Close,Volume\n";

    // Act
    var result = sut.Parse<T>(csv);

    // Assert
    Assert.That(result, Is.Empty);
  }

  [Test]
  public void Parse_InvalidRows_AreSkipped()
  {
    // Arrange
    string csv = "Date,Open,High,Low,Close,Volume\n" +
                 "INVALID,foo,bar,baz,notANumber,NaN\n" +
                 "2023-01-03,120.1,125.0,119.0,122.5,1500000";

    // Act
    var result = sut.Parse<T>(csv);

    // Assert
    Assert.That(result, Has.Count.EqualTo(1));
    Assert.That(result[0].Date, Is.EqualTo(new DateTime(2023, 1, 3)));
  }

  [Test]
  public void Parse_CsvWithExtraWhitespace_ParsesCorrectly()
  {
    // Arrange
    string csv = "Date,Open,High,Low,Close,Volume\n" +
                 " 2023-01-04 , 130.0 , 135.0 , 128.5 , 133.2 , 2500000 ";

    // Act
    var result = sut.Parse<T>(csv);

    // Assert
    Assert.Multiple(() =>
    {
      Assert.That(result, Has.Count.EqualTo(1));
      Assert.That(result[0].Close, Is.EqualTo(133.2));
      Assert.That(result[0].Volume, Is.EqualTo(2500000));
    });
  }
}
