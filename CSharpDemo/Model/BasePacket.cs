namespace CSharpDemo.Model
{
    public class BasePacket
    {
        public const string TimeOid = "10000051";
        public const string CellOid = "60000020";
        public const string ExceptionOid = "60000009";
        
        public string Oid { get; set; }

        /// <summary>
        /// 单个数据长度，不包括Oid和Length字段
        /// </summary>
        public int Length { get; set; }

        public byte[] DataValue { get; set; }
    }
}