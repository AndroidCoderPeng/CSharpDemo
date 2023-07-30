using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using CSharpDemo.Tags;
using PixelFormat = System.Windows.Media.PixelFormat;

namespace CSharpDemo.Utils
{
    public static class MethodExtensions
    {
        public static byte[] ToBytes(this Bitmap bitmap)
        {
            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Jpeg);
            var buffer = ms.ToArray();
            ms.Close();
            ms.Dispose();
            return buffer;
        }

        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            var bitImage = new BitmapImage();
            bitImage.BeginInit();
            bitImage.StreamSource = ms;
            bitImage.EndInit();
            return bitImage;
        }

        public static void ToImageFile(this Bitmap bitmap, string dirPath)
        {
            bitmap.Save(dirPath + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg", ImageFormat.Jpeg);
        }

        /// <summary>
        /// 图片
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static FormatConvertedBitmap ToFormatConvertedBitmap(this Uri uri, PixelFormat format)
        {
            var bitmapImage = new BitmapImage(uri);

            var formatConvertedBitmap = new FormatConvertedBitmap();
            formatConvertedBitmap.BeginInit();
            formatConvertedBitmap.Source = bitmapImage;
            formatConvertedBitmap.DestinationFormat = format;
            formatConvertedBitmap.EndInit();

            return formatConvertedBitmap;
        }

        /// <summary>
        /// 数据库UserId转为8位16进制
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string ToHex8(this int id)
        {
            return id.ToString("G").PadLeft(8, '0');
        }

        /// <summary>
        /// 8位16进制转为数据库UserId
        /// </summary>
        /// <param name="hexId"></param>
        /// <returns></returns>
        public static int ToIntId(this string hexId)
        {
            return Convert.ToInt32(hexId);
        }

        /// <summary>
        /// byte[]转int
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static int ToInt(this byte[] bytes)
        {
            var hex = BitConverter.ToString(bytes).Replace("-", "");
            return Convert.ToInt32(hex, 16);
        }

        private static readonly Dictionary<string, Uri> UriDictionary = new Dictionary<string, Uri>();

        public static Uri CreateUri(this string xamlName)
        {
            if (xamlName.IsUriExist())
            {
                return UriDictionary[xamlName];
            }

            var uri = new Uri("/Pages/" + xamlName + ".xaml", UriKind.Relative);
            UriDictionary[xamlName] = uri;
            return uri;
        }

        private static bool IsUriExist(this string xamlName)
        {
            return UriDictionary.Any(
                keyValuePair => keyValuePair.Key.Equals(xamlName)
            );
        }

        public static string ConvertBytes2String(this IEnumerable<byte> bytes)
        {
            return bytes.Aggregate("", (current, t) => current + t.ToString("X2"));
        }

        public static string GetOpeTypeByPdu(this byte[] pduType)
        {
            pduType = pduType.Reverse().ToArray();
            var btPduType = BitConverter.ToInt16(pduType, 0);

            var operaType = (btPduType >> 8) & 0xFF;
            string result;
            switch (operaType)
            {
                case 1:
                    result = "GetRequest";
                    break;
                case 2:
                    result = "GetResponse";
                    break;
                case 3:
                    result = "SetRequest";
                    break;
                case 4:
                    result = "TrapRequest";
                    break;
                case 5:
                    result = "TrapResponse";
                    break;
                case 6:
                    result = "OnlineRequest";
                    break;
                case 7:
                    result = "OnlineResponse";
                    break;
                case 8:
                    result = "StartupRequest";
                    break;
                case 9:
                    result = "StartupResponse";
                    break;
                case 10:
                    result = "WakeupRequest";
                    break;
                case 11:
                    result = "WakeupResponse";
                    break;
                case 13:
                    result = "ClientRequest";
                    break;
                case 12:
                    result = "SetResponse";
                    break;
                default:
                    result = "undefined";
                    break;
            }

            return result;
        }

        public static List<Tag> GetTags(this byte[] strTags)
        {
            var tags = new List<Tag>();
            var i = 0;
            while (i < strTags.Length)
            {
                var oid = new byte[4];
                var len = new byte[2];
                Array.Copy(strTags, i, oid, 0, 4);
                Array.Copy(strTags, i + 4, len, 0, 2);

                Array.Reverse(len);
                int iLen = BitConverter.ToInt16(len, 0);
                var strOid = oid.ConvertBytes2String();

                var value = new byte[iLen];
                Array.Copy(strTags, i + 6, value, 0, iLen);

                i = i + 6 + iLen;
                var tag = TagFactory.Create(strOid, iLen, value);
                tags.Add(tag);
            }

            return tags;
        }

        public static string AppendLeftZero(this int i)
        {
            //数据固定长度2
            return i.ToString("G").PadLeft(2, '0');
        }

        public static string ToChineseType(this int i)
        {
            //1,2,3,4分别代表流量、压力、液位、噪声
            switch (i)
            {
                case 1:
                    return "流量数据";
                case 2:
                    return "压力数据";
                case 3:
                    return "液位数据";
                case 4:
                    return "噪声数据";
                default:
                    return "未知类型数据";
            }
        }
    }
}