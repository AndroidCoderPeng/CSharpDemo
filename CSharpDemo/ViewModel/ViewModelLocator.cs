/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using CSharpDemo.Service;
using CSharpDemo.ServiceImpl;
using GalaSoft.MvvmLight.Ioc;

namespace CSharpDemo.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            #region Service

            SimpleIoc.Default.Register<IMainDataService, MainDataServiceImpl>();

            #endregion

            #region VM

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<UdpServerViewModel>();
            SimpleIoc.Default.Register<CircleLoadingViewModel>();
            SimpleIoc.Default.Register<TransmitValueViewModel>();

            #endregion
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public UdpServerViewModel UdpServer => ServiceLocator.Current.GetInstance<UdpServerViewModel>();
        public CircleLoadingViewModel CircleLoading => ServiceLocator.Current.GetInstance<CircleLoadingViewModel>();
        public TransmitValueViewModel TransmitValue => ServiceLocator.Current.GetInstance<TransmitValueViewModel>();

        public static void Cleanup()
        {
        }
    }
}