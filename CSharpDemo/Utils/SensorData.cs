namespace CSharpDemo.Utils
{
    public class SensorDataWrapper
    {
        public string DataType { get; set; }

        public (double[], double[]) FirstSensor { get; set; }
        public (double[], double[]) SecondSensor { get; set; }
    }
}