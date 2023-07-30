using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CSharpDemo.Tags;
using CSharpDemo.Utils;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace CSharpDemo.Pages
{
    public partial class DataAnalysisPage : Page
    {
        public DataAnalysisPage()
        {
            InitializeComponent();

            ImportDataButton.Click += delegate
            {
                var fileDialog = new OpenFileDialog
                {
                    // 设置默认格式
                    DefaultExt = ".txt",
                    Filter = "水听器数据文件(*.txt)|*.txt"
                };
                var result = fileDialog.ShowDialog();
                if (result == true)
                {
                    var fileName = fileDialog.FileName;
                    DataFilePathTextBox.Text = fileName;
                    var streamReader = new StreamReader(fileName);
                    var data = streamReader.ReadLine();
                    streamReader.Close();
                    if (data == null)
                    {
                        return;
                    }

                    OriginalDataTextBox.Text = data;
                    OriginalDataTextBlock.Text = $"原始数据长度：{data.Length / 2} 字节";

                    //将string转为byte[]
                    var temp = new List<string>();
                    for (var i = 0; i < data.Length; i += 2)
                    {
                        temp.Add(data.Substring(i, 2));
                    }

                    var bytes = new byte[temp.Count];
                    for (var i = 0; i < temp.Count; i++)
                    {
                        bytes[i] = Convert.ToByte(temp[i], 16);
                    }

                    //测试是否转化成功
                    if (!data.Equals(BitConverter.ToString(bytes).Replace("-", "")))
                    {
                        MessageBox.Show("数据转化失败!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    HandleSerialPortData(bytes);
                }
            };
        }

        private void HandleSerialPortData(byte[] bytes)
        {
            var deviceIdBytes = new byte[6];
            Array.Copy(bytes, 4, deviceIdBytes, 0, 6);
            var deviceId = deviceIdBytes.ConvertBytes2String();

            DeviceIdTextBlock.Text = $"设备ID：{deviceId}";

            var pduTypeBytes = new byte[2];
            Array.Copy(bytes, 13, pduTypeBytes, 0, 2);
            var operateType = pduTypeBytes.GetOpeTypeByPdu();

            PduTypeTextBlock.Text = $"{operateType}";

            var tagBytes = new byte[bytes.Length - 18];
            Array.Copy(bytes, 16, tagBytes, 0, bytes.Length - 18);
            var tags = tagBytes.GetTags();
            var tagBuilder = new StringBuilder();
            for (var i = 0; i < tags.Count; i++)
            {
                var tag = tags[i];
                switch (tag)
                {
                    case CellTag _:
                        tagBuilder.Append("CellTag").Append(", ");
                        break;
                    case TimeTag _:
                        tagBuilder.Append("TimeTag").Append(", ");
                        break;
                    case UploadTag _ when i == tags.Count - 1:
                        tagBuilder.Append("UploadTag");
                        break;
                    case UploadTag _:
                        tagBuilder.Append("UploadTag").Append(", ");
                        break;
                    default:
                    {
                        if (i == tags.Count - 1)
                        {
                            tagBuilder.Append("NormalTag");
                        }
                        else
                        {
                            tagBuilder.Append("NormalTag").Append(", ");
                        }

                        break;
                    }
                }
            }

            TagTypeTextBlock.Text = tagBuilder.ToString();

            //其实就3个Tag，[CellTag,TimeTag,UploadTag]
            var noiseTag = tags.Where(tag => tag is UploadTag).Cast<UploadTag>().First();
            if (noiseTag != null)
            {
                //理论上noiseTag不会为空
                var num = noiseTag.Len / 3;
                var realData = new double[num];
                for (var i = 0; i < num; i++)
                {
                    var dStr = new byte[3];
                    Array.Copy(noiseTag.DataValue, i * 3, dStr, 0, 3);

                    realData[i] = dStr.HexToDouble();
                }

                HandledDataTextBox.Text = JsonConvert.SerializeObject(realData);
            }
            else
            {
                Debug.WriteLine("DataAnalysisPage.xaml => noiseTag == null");
            }
        }
    }
}