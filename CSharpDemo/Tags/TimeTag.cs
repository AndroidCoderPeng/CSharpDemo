using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CSharpDemo.Utils;

namespace CSharpDemo.Tags
{
    public class TimeTag : Tag
    {
        public string Time { get; set; }

        public TimeTag(string oid, int len, byte[] dataValue) : base(oid, len, dataValue)
        {
            var hex = BitConverter.ToString(dataValue).Replace("-", "");
            var temp = new List<string>();
            for (var i = 0; i < hex.Length; i += 2)
            {
                temp.Add(hex.Substring(i, 2));
            }

            var timeBuilder = new StringBuilder();
            var year = $"{Convert.ToInt32(temp[0], 16) + 2000}";
            var month = Convert.ToInt32(temp[1], 16).AppendLeftZero();
            var day = Convert.ToInt32(temp[2], 16).AppendLeftZero();
            var hour = Convert.ToInt32(temp[3], 16).AppendLeftZero();
            var minute = Convert.ToInt32(temp[4], 16).AppendLeftZero();
            var seconds = Convert.ToInt32(temp[5], 16).AppendLeftZero();
            timeBuilder.Append(year).Append(month).Append(day).Append(hour).Append(minute).Append(seconds);
            Time = timeBuilder.ToString();
            Debug.WriteLine($"TimeTag => [Oid:{oid}, Time:{Time}]");
        }
    }
}