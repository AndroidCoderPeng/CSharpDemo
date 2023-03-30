using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
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
    }
}