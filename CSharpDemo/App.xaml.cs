using System.Windows;
using CSharpDemo.Service;
using CSharpDemo.ViewModels;
using CSharpDemo.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;

namespace CSharpDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            var mainWindow = Container.Resolve<MainWindow>();
            mainWindow.Loaded += delegate
            {
                var regionManager = Container.Resolve<IRegionManager>();
                regionManager.RequestNavigate("ContentRegion", "CameraView");
            };
            return mainWindow;
        }

        /// <summary>
        /// 通过IOC注册一些数据、服务等
        /// </summary>
        /// <param name="containerRegistry"></param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //Data
            containerRegistry.Register<IAppDataService, AppDataServiceImpl>();

            //Navigation
            containerRegistry.RegisterForNavigation<CameraView, CameraViewModel>();
            containerRegistry.RegisterForNavigation<TransmitValueView, TransmitValueViewModel>();
            containerRegistry.RegisterForNavigation<SerialPortView, SerialPortViewModel>();
            containerRegistry.RegisterForNavigation<DataAnalysisView, DataAnalysisViewModel>();
            containerRegistry.RegisterForNavigation<AudioWaveView, AudioWaveViewModel>();
            containerRegistry.RegisterForNavigation<AlgorithmTestView, AlgorithmTestViewModel>();
        }
    }
}