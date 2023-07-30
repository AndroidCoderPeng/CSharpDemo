using System;
using System.Diagnostics;

namespace CSharpDemo.Tags
{
    public class SensorExceptionTag : Tag
    {
        public string State { get; set; }

        public SensorExceptionTag(string oid, int len, byte[] dataValue) : base(oid, len, dataValue)
        {
            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $" SensorExceptionTag => {oid}");
            var intState = dataValue[0];
            State = intState + "";
        }
    }
}