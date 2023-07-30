namespace CSharpDemo.Tags
{
    public abstract class Tag
    {
        public string Oid { get; set; }
        public int Len { get; set; }
        public byte[] DataValue { get; set; }

        protected Tag(string oid, int len, byte[] dataValue)
        {
            Oid = oid;
            Len = len;
            DataValue = dataValue;
        }

        protected Tag()
        {
        }
    }
}