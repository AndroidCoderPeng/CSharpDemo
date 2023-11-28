using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using CSharpDemo.Tags;
using MathWorks.MATLAB.NET.Arrays;
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
        /// 字节数组转Int
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static int ConvertToInt(this byte[] bytes)
        {
            return bytes.Aggregate(0, (current, b) => 16 * 16 * current + b);
        }

        /// <summary>
        /// 字节数组转String
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static string ConvertToString(this byte[] bytes)
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

        /// <summary>
        /// 通过Linq查找噪声Tag
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static UploadTag GetUploadNoiseTag(this IEnumerable<Tag> tags)
        {
            return tags.Where(tag => tag is UploadTag).Cast<UploadTag>().FirstOrDefault();
        }

        public static List<Tag> GetTags(this byte[] tagBytes)
        {
            var tags = new List<Tag>();
            var i = 0;
            // var n = 0;
            while (i < tagBytes.Length)
            {
                // n++;
                var oidBytes = new byte[4];
                Array.Copy(tagBytes, i, oidBytes, 0, 4);
                var oid = oidBytes.ConvertToString();

                //value域的长度
                var tagValueBytes = new byte[2];
                Array.Copy(tagBytes, i + 4, tagValueBytes, 0, 2);
                Array.Reverse(tagValueBytes);
                int tagValueLength = BitConverter.ToInt16(tagValueBytes, 0);
                // Console.WriteLine($@"value域的长度 => {tagValueLength}");

                var valueBytes = new byte[tagValueLength];
                Array.Copy(tagBytes, i + 6, valueBytes, 0, tagValueLength);
                // Console.WriteLine($@"第{n}个Tag => {BitConverter.ToString(valueBytes)}");

                i = i + 6 + tagValueLength;
                var tag = TagFactory.Create(oid, tagValueLength, valueBytes);
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

        public static double HexToDouble(this IReadOnlyList<byte> src)
        {
            if (src.Count != 3)
                return 0;

            short result1 = src[0];
            short result2 = src[1];
            short result3 = src[2];

            if ((result1 & 0x80) == 0x80)
            {
                result1 = Convert.ToInt16(result1 - 255);
                result2 = Convert.ToInt16(result2 - 255);
                result3 = Convert.ToInt16(result3 - 255);
            }

            return (result1 * 65536 + result2 * 256 + result3) * 5 / 83.88607 / 100000;
        }

        public static List<string> ReadFromFile(this string filePath)
        {
            var list = new List<string>();
            var streamReader = new StreamReader(filePath);
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                list.Add(line);
            }

            streamReader.Close();
            return list;
        }

        /// <summary>
        /// MWNumericArray转double[]
        /// </summary>
        /// <param name="inputMw"></param>
        /// <returns></returns>
        public static double[] GetArray(this MWNumericArray inputMw)
        {
            var num = inputMw.NumberOfElements;
            var outArray = new double[num];
            for (var i = 0; i < num; i++)
            {
                outArray[i] = Convert.ToDouble(inputMw[i + 1].ToString());
            }

            return outArray;
        }

        public static void SaveArrayToFile(this double[] array, string fileName)
        {
            var builder = new StringBuilder();
            foreach (var d in array)
            {
                builder.Append(d).Append("\r\n");
            }

            File.AppendAllText(fileName, builder.ToString());
        }
    }
}