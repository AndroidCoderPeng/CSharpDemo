using System.Diagnostics;

namespace CSharpDemo.Tags
{
    public class NormalTag : Tag
    {
        public NormalTag(string oid, int len, byte[] dataValue) : base(oid, len, dataValue)
        {
            Debug.WriteLine($"NormalTag => {oid}");
        }
    }
}