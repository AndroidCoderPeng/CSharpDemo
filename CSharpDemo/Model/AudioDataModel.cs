namespace CSharpDemo.Model
{
    // 时域
    public class TimeDomainData
    {
        public double[] TimeAxis { get; set; }
        public double[] Amplitude { get; set; }
    }
    
    // 频域
    public class FrequencyDomainData
    {
        public double[] Frequencies { get; set; }
        public double[] Magnitudes { get; set; }
    }
}