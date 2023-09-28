using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using CorrelatorSingle;
using CSharpDemo.Model;
using CSharpDemo.Tags;
using CSharpDemo.Utils;
using CSharpDemo.Views;
using MathWorks.MATLAB.NET.Arrays;
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

        #endregion

        #region DelegateCommand

        public DelegateCommand<DataAnalysisView> WindowLoadedCommand { get; }
        public DelegateCommand ImportRedDataCommand { get; }
        public DelegateCommand ImportBlueDataCommand { get; }

        #endregion

        private DataAnalysisView _view;
        private bool _isRedSensor;
        private readonly CorrelatorDataModel _dataModel;
        private readonly BackgroundWorker _backgroundWorker;
        private static readonly Lazy<Correlator> LazyCorrelator = new Lazy<Correlator>(() => new Correlator());

        public DataAnalysisViewModel()
        {
            _dataModel = new CorrelatorDataModel();

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += Worker_OnDoWork;
            _backgroundWorker.ProgressChanged += Worker_OnProgressChanged;
            _backgroundWorker.RunWorkerCompleted += Worker_OnRunWorkerCompleted;

            WindowLoadedCommand = new DelegateCommand<DataAnalysisView>(delegate(DataAnalysisView view)
            {
                _view = view;
            });

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

            //格式化double[]
            var totalData = new List<double>();
            for (var i = 0; i < doubleArrays.Count; i++)
            {
                totalData.AddRange(doubleArrays[i]);

                var percent = (i + 1) / (float)doubleArrays.Count;
                _backgroundWorker.ReportProgress((int)(percent * 100));
                Thread.Sleep(10);
            }

            var resultArray = totalData.ToArray();
            if (_isRedSensor)
            {
                _dataModel.DevCode = "211700082201";
                _dataModel.LeftReceiveDataTime = DateTime.Now;
                _dataModel.LeftDeviceDataArray = resultArray;

                RedHandledData = "数据渲染中...";
                RedHandledData = JsonConvert.SerializeObject(resultArray);
            }
            else
            {
                _dataModel.DevCode = "211700082202";
                _dataModel.RightReceiveDataTime = DateTime.Now;
                _dataModel.RightDeviceDataArray = resultArray;

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
            if (_dataModel.LeftDeviceDataArray == null || _dataModel.RightDeviceDataArray == null)
            {
                return;
            }

            new Thread(CalculateData).Start();
        }

        private void CalculateData()
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                DialogHub.Get.ShowLoadingDialog(Window.GetWindow(_view), "数据计算中，请稍后...");
            });
            Console.WriteLine(@"DataAnalysisViewModel => 开始计算");
            var array = LazyCorrelator.Value.locating(11,
                (MWNumericArray)_dataModel.LeftDeviceDataArray, (MWNumericArray)_dataModel.RightDeviceDataArray,
                7500,
                int.Parse("150"), int.Parse("1130"),
                0, 0,
                0, 0,
                "",
                int.Parse("300"), int.Parse("300"),
                1, -1,
                -1, -1,
                int.Parse("10"), int.Parse("300"));
            Console.WriteLine(@"DataAnalysisViewModel => 计算结束");

            Application.Current.Dispatcher.Invoke(delegate
            {
                //渲染波形图
                Console.WriteLine(@"DataAnalysisView.xaml => 开始渲染波形图");

                //最大相关系数  
                var maxCorrelationCoefficient = Convert.ToDouble(array[3].ToString());

                var xDoubles = ((MWNumericArray)array[5]).GetArray();
                var yDoubles = ((MWNumericArray)array[4]).GetArray();

                var timeDiff = Convert.ToDouble(array[6].ToString());
                Console.WriteLine($@"时间差 => {timeDiff}");

                var chart = _view.ScottplotView;
                chart.Plot.SetAxisLimits(0, xDoubles.Last(), 0, maxCorrelationCoefficient);
                chart.Plot.AddFill(
                    xDoubles, yDoubles,
                    color: Color.FromArgb(255, 49, 151, 36),
                    lineWidth: 0.1f,
                    lineColor: Color.FromArgb(255, 49, 151, 36)
                );

                chart.Plot.AxisAuto();
                chart.Refresh();

                DialogHub.Get.DismissLoadingDialog();
            });
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