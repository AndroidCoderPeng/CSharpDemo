using System.Diagnostics;
using System.Globalization;
using CSharpDemo.Utils;

namespace CSharpDemo.Tags
{
    public class UploadTag : Tag
    {
        /**
         * 1,2,3,4,5,分别代表流量、压力、液位、噪声
         */
        public int BizType { get; set; } //业务类型

        public int CollectInter { get; set; } //采集间隔(单位是分钟)

        public string CollectTime { get; set; } //采集时间（15时）

        /*
        * if oid begins with 1100 return true
        */
        public static bool IsUploadTag(string oid)
        {
            var intOid = int.Parse(oid, NumberStyles.HexNumber);
            var temp = (intOid >> 28) & 0x0F;
            return temp == 0x0C;
        }

        public UploadTag(string oid, int len, byte[] dataValue) : base(oid, len, dataValue)
        {
            var btPduType = int.Parse(Oid, NumberStyles.HexNumber);
            BizType = (btPduType >> 24) & 0x0F;

            //转换采集间隔
            CollectInter = (btPduType >> 11) & 0x7FF;

            var collectMin = btPduType & 0x7FF;
            var minute = collectMin % 60;
            var hour = collectMin / 60;
            CollectTime = hour + ":" + minute + ":00";

            Debug.WriteLine($"UploadTag => [Oid:{oid}, BizType:{BizType.ToChineseType()}]");
        }
    }
}