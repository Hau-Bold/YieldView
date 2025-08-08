using Microsoft.Extensions.Options;
using System.Globalization;
using System.Xml.Linq;
using YieldView.API.Configurations;
using YieldView.API.Models;
using YieldView.API.Services.Contract;

namespace YieldView.API.Services.Impl
{
    public class TreasuryXmlService : ITreasuryXmlService
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _sourceUrls;

        public TreasuryXmlService(HttpClient httpClient, IOptions<YieldCurveSourceConfig> options)
        {
            _httpClient = httpClient;
            _sourceUrls = options.Value.ToDictionary();
        }

        public async Task<List<YieldCurvePoint>> DownloadAndParseYieldCurveAsync(string country, int year)
        {
            if (!_sourceUrls.TryGetValue(country.ToUpper(), out var baseUrl))
            {
                throw new ArgumentException($"No URL configured for country code '{country}'");
            }

            var fullUrl = $"{baseUrl}={year}";
            var xml = await _httpClient.GetStringAsync(fullUrl);

            var doc = XDocument.Parse(xml);
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

            var entries = new List<YieldCurvePoint>();

            foreach (var entry in doc.Descendants("entry"))
            {
                var props = entry.Descendants(m + "properties").FirstOrDefault();
                if (props == null) continue;

                var dateStr = props.Element(d + "NEW_DATE")?.Value;
                if (!DateTime.TryParse(dateStr, out var date)) continue;

                // Loop through all children in <m:properties>
                foreach (var element in props.Elements())
                {
                    var localName = element.Name.LocalName;
                    if (!localName.StartsWith("BC_") || localName == "BC_30YEARDISPLAY")
                        continue;

                    var maturity = localName.Replace("BC_", "")   
                                            .Replace("YEAR", "Y")  
                                            .Replace("MONTH", "M");

                    if (double.TryParse(element.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var yield))
                    {
                        entries.Add(new YieldCurvePoint
                        {
                            Date = date,
                            Country = country.ToUpper(),
                            Maturity = maturity,
                            Yield = yield
                        });
                    }
                }
            }

            return entries;
        }
    }
}
