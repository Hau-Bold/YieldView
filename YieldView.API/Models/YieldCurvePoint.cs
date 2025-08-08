namespace YieldView.API.Models
{
    public class YieldCurvePoint
    {
        public int Id { get; set; }
        public string Country { get; set; } =  string.Empty;
        public DateTime Date { get; set; }
        public string Maturity { get; set; } = string.Empty;
        public double Yield { get; set; }
    }
}
