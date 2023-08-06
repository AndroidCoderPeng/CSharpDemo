using System;

namespace CSharpDemo.Model
{
    public class CorrelatorDataModel
    {
        public string DevCode { get; set; }
        public double[] LeftDeviceDataArray { get; set; }
        public DateTime LeftReceiveDataTime { get; set; }
        public double[] RightDeviceDataArray { get; set; }
        public DateTime RightReceiveDataTime { get; set; }
    }
}