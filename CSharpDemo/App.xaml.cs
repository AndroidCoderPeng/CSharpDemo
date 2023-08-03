using System.Windows;
using CSharpDemo.Service;
using CSharpDemo.ServiceImpl;
using CSharpDemo.Views;
using Prism.DryIoc;
using Prism.Ioc;

namespace CSharpDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        /// <summary>
        /// 初始化Shell（主窗口）会执行这个方法
        /// </summary>
        /// <param name="shell"></param>
        protected override void InitializeShell(Window shell)
        {
            //可以实现登录
            // var window = Container.Resolve<LoginWindow>();
            // if (window == null || window.ShowDialog() != true)
            // {
            //     Current.Shutdown();
            // }
            // else
            // {
            //     base.InitializeShell(shell);
            // }
        }

        /// <summary>
        /// 通过IOC注册一些数据、服务等
        /// </summary>
        /// <param name="containerRegistry"></param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //Data
            containerRegistry.Register<IMainDataService, MainDataServiceImpl>();
            
            //Navigation
            containerRegistry.RegisterForNavigation<CameraView>();
            containerRegistry.RegisterForNavigation<ScottPlotView>();
            containerRegistry.RegisterForNavigation<TransmitValueView>();
            containerRegistry.RegisterForNavigation<SerialPortView>();
            containerRegistry.RegisterForNavigation<DataAnalysisView>();
        }
    }
}