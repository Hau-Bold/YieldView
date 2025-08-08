namespace YieldView.API.Models
{
    public class YieldCurve
    {
        public int Id { get; set; }
        public string Country { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public ICollection<YieldCurvePoint> Points { get; set; } = [];
    }
}
