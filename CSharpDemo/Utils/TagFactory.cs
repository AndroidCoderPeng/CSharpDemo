using CSharpDemo.Tags;

namespace CSharpDemo.Utils
{
    public static class TagFactory
    {
        public static Tag Create(string oid, int len, byte[] value)
        {
            Tag tag;
            if (UploadTag.IsUploadTag(oid))
            {
                //数据存在此Tag中
                tag = new UploadTag(oid, len, value);
            }
            else
            {
                switch (oid)
                {
                    case TagOid.CellTagOid:
                        tag = new CellTag(oid, len, value);
                        break;
                    case TagOid.TimeTagOid:
                        tag = new TimeTag(oid, len, value);
                        break;
                    case TagOid.ExceptionTagOid:
                        tag = new SensorExceptionTag(oid, len, value);
                        break;
                    default:
                        tag = new NormalTag(oid, len, value);
                        break;
                }
            }

            return tag;
        }
    }
}