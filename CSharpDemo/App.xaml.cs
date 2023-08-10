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
            var loginWindow = Container.Resolve<LoginWindow>();
            var loginResult = loginWindow.ShowDialog();
            if (loginResult == null)
            {
                Current.Shutdown();
                return;
            }

            if (loginResult.Value)
            {
                base.OnInitialized();
            }
            else
            {
                Current.Shutdown();
            }
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
            containerRegistry.RegisterForNavigation<TransmitValueView>();
            containerRegistry.RegisterForNavigation<SerialPortView>();
            containerRegistry.RegisterForNavigation<DataAnalysisView>();
            containerRegistry.RegisterForNavigation<AudioFileToWaveView>();
            containerRegistry.RegisterForNavigation<RealTimeAudioView>();
        }
    }
}