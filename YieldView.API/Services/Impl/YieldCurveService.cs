using System.Text.Json;
using YieldView.API.Models;
using YieldView.API.Services.Contract;

namespace YieldView.API.Services.Impl;

// TODO: Seems unused
  public class YieldCurveService : IYieldCurveService
  {
      private readonly HttpClient _httpClient;

      public async Task<List<YieldCurvePoint>> FetchYieldCurveAsync(string url, string country)
      {
          try
          {
              var response = await _httpClient.GetAsync(url);

              if (!response.IsSuccessStatusCode)
              {
                  return null;
              }

              var json = await response.Content.ReadAsStringAsync();


              var points = ParseYieldCurveFromJson(json, country);
              return points;
          }
          catch (Exception ex)
          {
              return null;
          }
      }

      private List<YieldCurvePoint> ParseYieldCurveFromJson(string json, string country)
      {
         
          try
          {
              var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

              var rawPoints = JsonSerializer.Deserialize<List<RawYieldPoint>>(json, options);

              if (rawPoints == null)
                  return new List<YieldCurvePoint>();

              return rawPoints.Select(r => new YieldCurvePoint
              {
                  Country = country,
                  Maturity = r.Maturity,
                  Yield = r.Yield
              }).ToList();
          }
          catch
          {
              return new List<YieldCurvePoint>();
          }
      }

      private class RawYieldPoint
      {
          public string Maturity { get; set; }
          public double Yield { get; set; }
      }
  }
