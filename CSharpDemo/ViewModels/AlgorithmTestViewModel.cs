using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CorrelatorSingle;
using CSharpDemo.Events;
using CSharpDemo.Model;
using CSharpDemo.Utils;
using MathWorks.MATLAB.NET.Arrays;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace CSharpDemo.ViewModels
{
    public class AlgorithmTestViewModel : BindableBase
    {
        #region VM

        private string _configFilePath;

        public string ConfigFilePath
        {
            get => _configFilePath;
            set
            {
                _configFilePath = value;
                RaisePropertyChanged();
            }
        }

        private ParamConfig _config = new ParamConfig();

        public ParamConfig Config
        {
            get => _config;
            set
            {
                _config = value;
                RaisePropertyChanged();
            }
        }

        private string _sensorDataPath;

        public string SensorDataPath
        {
            get => _sensorDataPath;
            set
            {
                _sensorDataPath = value;
                RaisePropertyChanged();
            }
        }

        private double[] _firstSensorData;

        public double[] FirstSensorData
        {
            get => _firstSensorData;
            set
            {
                _firstSensorData = value;
                RaisePropertyChanged();
            }
        }

        private double[] _secondSensorData;

        public double[] SecondSensorData
        {
            get => _secondSensorData;
            set
            {
                _secondSensorData = value;
                RaisePropertyChanged();
            }
        }

        private bool _isCalculateEnabled;

        public bool IsCalculateEnabled
        {
            get => _isCalculateEnabled;
            set
            {
                _isCalculateEnabled = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region DelegateCommand

        public DelegateCommand SelectParamConfigFileCommand { get; set; }
        public DelegateCommand ImportSensorDataCommand { set; get; }
        public DelegateCommand StartCalculateCommand { set; get; }

        #endregion

        private readonly IEventAggregator _eventAggregator;
        private readonly BackgroundWorker _backgroundWorker;
        private static readonly Lazy<Correlator> LazyCorrelator = new Lazy<Correlator>(() => new Correlator());
        private int _soundSpeed = 1130;
        private CorrelatorData _sensorData;

        public AlgorithmTestViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += Worker_OnDoWork;
            _backgroundWorker.RunWorkerCompleted += Worker_OnRunWorkerCompleted;

            SelectParamConfigFileCommand = new DelegateCommand(SelectParamConfigFile);
            ImportSensorDataCommand = new DelegateCommand(ImportSensorData);
            StartCalculateCommand = new DelegateCommand(CalculateData);
        }

        private void SelectParamConfigFile()
        {
            var fileDialog = new OpenFileDialog
            {
                // 设置默认格式
                DefaultExt = ".json",
                Filter = "参数配置文件(*.json)|*.json"
            };
            var result = fileDialog.ShowDialog();
            if (result != true) return;

            ConfigFilePath = fileDialog.FileName;
            var fromFile = _configFilePath.ReadFromFile();
            //参数配置就一行
            var json = fromFile[0];
            if (json == null) return;
            // ParamConfig字段发生了变化，兼容旧版本的字段
            if (json.Contains("MinFrequency") && json.Contains("MaxFrequency"))
            {
                json = json.Replace("\"MinFrequency\"", "\"LowFrequency\"")
                    .Replace("\"MaxFrequency\"", "\"HighFrequency\"");
            }

            Console.WriteLine(json);
            var config = JsonConvert.DeserializeObject<ParamConfig>(json);

            //计算声速
            // _soundSpeed = _dataService.GetSoundVelocity(config.PipeMaterial, Convert.ToInt32(config.PipeDiameter));
            // if (_soundSpeed == 0)
            // {
            //     MessageBox.Show("声速错误，请检查管材和管径参数", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Stop);
            //     return;
            // }

            Config = config;
        }

        private void ImportSensorData()
        {
            var fileDialog = new OpenFileDialog
            {
                // 设置默认格式
                DefaultExt = ".txt",
                Filter = "传感器数据文件(*.txt)|*.txt"
            };
            var result = fileDialog.ShowDialog();
            if (result != true) return;
            SensorDataPath = fileDialog.FileName;
            //开始处理数据
            if (_backgroundWorker.IsBusy)
            {
                // 先取消再执行
                _backgroundWorker.CancelAsync();
            }

            IsCalculateEnabled = false;
            _backgroundWorker.RunWorkerAsync();
        }

        private void Worker_OnDoWork(object sender, DoWorkEventArgs e)
        {
            //开始处理数据
            var list = _sensorDataPath.ReadFromFile();
            list.RemoveAt(0); // 去掉第1行时间
            var index = list.FindIndex(item => item == "==============="); // 找到分割线

            var first = list.GetRange(0, index); // 分割线前的数据（第一个传感器）
            var second = list.GetRange(index + 1, list.Count - index - 1); // 分割线后的数据（第二个传感器）

            // 转为double[]
            FirstSensorData = first.Select(double.Parse).ToArray();
            SecondSensorData = second.Select(double.Parse).ToArray();

            var redNoiseValue = 0.0;
            Array.ForEach(_firstSensorData, i => redNoiseValue += Math.Abs(i));

            var blueNoiseValue = 0.0;
            Array.ForEach(_secondSensorData, i => blueNoiseValue += Math.Abs(i));

            // 将数据传递给Worker_OnRunWorkerCompleted
            var sensorData = new CorrelatorData
            {
                RedDevCode = RuntimeCache.Dev1,
                RedDeviceData = _firstSensorData,
                ReceiveRedSensorDataTime = DateTime.Now,
                RedSensorNoiseSumValue = redNoiseValue,

                BlueDevCode = RuntimeCache.Dev2,
                BlueDeviceData = _secondSensorData,
                ReceiveBlueSensorDataTime = DateTime.Now,
                BlueSensorNoiseSumValue = blueNoiseValue
            };

            // 将计算结果传递给Worker_OnRunWorkerCompleted
            e.Result = sensorData;
        }

        private void Worker_OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _sensorData = e.Result as CorrelatorData;
            IsCalculateEnabled = true;
        }

        /// <summary>
        /// 异步计算获得结果
        /// </summary>
        private void CalculateData()
        {
            Task.Run(() =>
            {
                // 直接开始计算
                var startTime = DateTime.Now;
                var array = LazyCorrelator.Value.locating(
                    11,
                    (MWNumericArray)_sensorData.RedDeviceData,
                    (MWNumericArray)_sensorData.BlueDeviceData,
                    7500,
                    int.Parse(_config.PipeLength), _soundSpeed,
                    0, 0,
                    0, 0,
                    _config.PipeMaterial,
                    int.Parse(_config.PipeDiameter), int.Parse(_config.PipeDiameter),
                    1, -1,
                    -1, -1,
                    int.Parse(_config.LowFrequency), int.Parse(_config.HighFrequency)
                );
                var endTime = DateTime.Now;
                var diffTime = Math.Abs((endTime - startTime).TotalMilliseconds) / 1000;
                Console.WriteLine($@"计算耗时 => {diffTime:F2}秒");

                _eventAggregator.GetEvent<CorrelatorResultEvent>().Publish(array);
            });
        }
    }
}