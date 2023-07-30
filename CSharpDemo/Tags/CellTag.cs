using System;
using System.Diagnostics;

namespace CSharpDemo.Tags
{
    public class CellTag : Tag
    {
        public string Cell { get; set; }

        public CellTag(string oid, int len, byte[] dataValue) : base(oid, len, dataValue)
        {
            var hex = BitConverter.ToString(dataValue).Replace("-", "");
            Cell = Convert.ToInt32(hex, 16).ToString();
            Debug.WriteLine($"CellTag => [Oid:{oid}, Cell:{Cell}]");
        }
    }
}