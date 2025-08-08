using YieldView.API.Models;

namespace YieldView.API.Services.Contract
{
    public interface ITreasuryXmlService
    {
        public Task<List<YieldCurvePoint>> DownloadAndParseYieldCurveAsync(string country, int year);
    }
}
