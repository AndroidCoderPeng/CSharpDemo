using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using CSharpDemo.Tags;
using CSharpDemo.Utils;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;

namespace CSharpDemo.ViewModels
{
    public class DataAnalysisViewModel : BindableBase
    {
        #region VM

        private string _redDataPath;

        public string RedDataPath
        {
            get => _redDataPath;
            set
            {
                _redDataPath = value;
                RaisePropertyChanged();
            }
        }

        private string _blueDataPath;

        public string BlueDataPath
        {
            get => _blueDataPath;
            set
            {
                _blueDataPath = value;
                RaisePropertyChanged();
            }
        }

        private string _redHandledData;

        public string RedHandledData
        {
            get => _redHandledData;
            set
            {
                _redHandledData = value;
                RaisePropertyChanged();
            }
        }

        private string _blueHandledData;

        public string BlueHandledData
        {
            get => _blueHandledData;
            set
            {
                _blueHandledData = value;
                RaisePropertyChanged();
            }
        }

        private int _progressBarValue;

        public int ProgressBarValue
        {
            get => _progressBarValue;
            set
            {
                _progressBarValue = value;
                RaisePropertyChanged();
            }
        }

        private int _maximumValue;

        public int MaximumValue
        {
            get => _maximumValue;
            set
            {
                _maximumValue = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region DelegateCommand

        public DelegateCommand ImportRedDataCommand { get; }
        public DelegateCommand ImportBlueDataCommand { get; }

        #endregion

        private bool _isRedSensor;

        private readonly BackgroundWorker _backgroundWorker;

        public DataAnalysisViewModel()
        {
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += Worker_OnDoWork;
            _backgroundWorker.ProgressChanged += Worker_OnProgressChanged;
            _backgroundWorker.RunWorkerCompleted += Worker_OnRunWorkerCompleted;

            ImportRedDataCommand = new DelegateCommand(delegate
            {
                _isRedSensor = true;
                var fileDialog = new OpenFileDialog
                {
                    // 设置默认格式
                    DefaultExt = ".txt",
                    Filter = "水听器数据文件(*.txt)|*.txt"
                };
                if (fileDialog.ShowDialog() == true)
                {
                    RedDataPath = fileDialog.FileName;

                    //开始处理数据
                    _backgroundWorker.RunWorkerAsync();
                }
            });

            ImportBlueDataCommand = new DelegateCommand(delegate
            {
                _isRedSensor = false;
                var fileDialog = new OpenFileDialog
                {
                    // 设置默认格式
                    DefaultExt = ".txt",
                    Filter = "水听器数据文件(*.txt)|*.txt"
                };
                if (fileDialog.ShowDialog() == true)
                {
                    BlueDataPath = fileDialog.FileName;

                    //开始处理数据
                    _backgroundWorker.RunWorkerAsync();
                }
            });
        }

        private void Worker_OnDoWork(object sender, DoWorkEventArgs e)
        {
            var fileName = _isRedSensor ? RedDataPath : BlueDataPath;

            var fromFile = fileName.ReadFromFile();
            var doubleArrays = fromFile.Select(HandleSerialPortData).ToList();
            MaximumValue = doubleArrays.Count;

            //格式化double[]
            var totalData = new List<double>();
            for (var i = 0; i < doubleArrays.Count; i++)
            {
                totalData.AddRange(doubleArrays[i]);
                _backgroundWorker.ReportProgress(i + 1);
                Thread.Sleep(10);
            }

            var resultArray = totalData.ToArray();
            if (_isRedSensor)
            {
                RedHandledData = "数据渲染中...";
                RedHandledData = JsonConvert.SerializeObject(resultArray);
            }
            else
            {
                BlueHandledData = "数据渲染中...";
                BlueHandledData = JsonConvert.SerializeObject(resultArray);
            }
        }

        private void Worker_OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBarValue = e.ProgressPercentage;
        }

        private void Worker_OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private List<double> HandleSerialPortData(string data)
        {
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
                return new List<double>();
            }

            var tagBytes = new byte[bytes.Length - 18];
            Array.Copy(bytes, 16, tagBytes, 0, bytes.Length - 18);
            var tags = tagBytes.GetTags();

            //其实就3个Tag，[CellTag,TimeTag,UploadTag]
            var noiseTag = tags.Where(tag => tag is UploadTag).Cast<UploadTag>().First();
            //理论上noiseTag不会为空
            var num = noiseTag.Len / 3;
            var realData = new List<double>();
            for (var i = 0; i < num; i++)
            {
                var dStr = new byte[3];
                Array.Copy(noiseTag.DataValue, i * 3, dStr, 0, 3);

                realData.Add(dStr.HexToDouble());
            }

            return realData;
        }
    }
}