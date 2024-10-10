using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CSharpDemo.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;

            // 移除当前主题  
            var dictionary = mergedDictionaries.Last();
            mergedDictionaries.Remove(dictionary);

            // 添加新主题  
            var newTheme = new ResourceDictionary { Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative) };
            mergedDictionaries.Add(newTheme);
        }

        private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;

            // 移除当前主题  
            var dictionary = mergedDictionaries.Last();
            mergedDictionaries.Remove(dictionary);

            // 添加新主题  
            var newTheme = new ResourceDictionary { Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative) };
            mergedDictionaries.Add(newTheme);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}