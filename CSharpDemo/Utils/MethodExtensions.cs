using System;
using System.Collections.Generic;
using System.IO;
using MathWorks.MATLAB.NET.Arrays;

namespace CSharpDemo.Utils
{
    public static class MethodExtensions
    {
        public static string AppendLeftZero(this int i)
        {
            //数据固定长度2
            return i.ToString("G").PadLeft(2, '0');
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
    }
}