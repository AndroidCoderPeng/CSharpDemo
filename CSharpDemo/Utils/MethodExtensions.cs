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

namespace CSharpDemo.Utils
{
    public static class MethodExtensions
    {
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

        /// <summary>
        /// 判断是否是2的幂
        /// </summary>
        public static bool IsPowerOfTwo(this int size)
        {
            return size > 0 && (size & (size - 1)) == 0;
        }

        public static double[] GetWeights(this int radius)
        {
            double Gaussian(double x) => Math.Pow(Math.E, -4 * x * x); // 高斯函数

            var len = 1 + radius * 2;
            var end = len - 1;
            var radiusF = (double)radius;
            var weights = new double[len];

            for (var i = 0; i <= radius; i++) // 先把右边的权重算出来
            {
                weights[radius + i] = Gaussian(i / radiusF);
            }

            for (var i = 0; i < radius; i++) // 把右边的权重拷贝到左边
            {
                weights[i] = weights[end - i];
            }

            var total = weights.Sum();
            for (var i = 0; i < len; i++) // 使权重合为 0
            {
                weights[i] /= total;
            }

            return weights;
        }

        /// <summary>
        /// 将幅度转换为分贝(dB)，用于专业频谱显示
        /// </summary>
        /// <param name="magnitudes"></param>
        /// <returns></returns>
        public static double[] ToDecibels(this double[] magnitudes)
        {
            var db = new double[magnitudes.Length];
            for (var i = 0; i < magnitudes.Length; i++)
            {
                db[i] = 20 * Math.Log10(Math.Max(magnitudes[i], 1e-10));
            }

            return db;
        }

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