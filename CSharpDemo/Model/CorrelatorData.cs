using System;

namespace CSharpDemo.Model
{
    public class CorrelatorData
    {
        public string RedDevCode { get; set; }
        public double[] RedDeviceData { get; set; }

        public DateTime ReceiveRedSensorDataTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 噪音总和值绝对值
        /// </summary>
        public double RedSensorNoiseSumValue { get; set; }

        public string BlueDevCode { get; set; }
        public double[] BlueDeviceData { get; set; }

        public DateTime ReceiveBlueSensorDataTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 噪音总和值绝对值
        /// </summary>
        public double BlueSensorNoiseSumValue { get; set; }
    }
}