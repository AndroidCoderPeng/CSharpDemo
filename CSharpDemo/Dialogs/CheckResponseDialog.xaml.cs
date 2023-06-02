using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CSharpDemo.Dialogs
{
    public partial class CheckResponseDialog : Window
    {
        public CheckResponseDialog()
        {
            InitializeComponent();

            GoBackButton.Click += delegate { Close(); };

            for (var i = 0; i < 60; i++)
            {
                var ellipse = new Ellipse
                {
                    Width = 25,
                    Height = 25,
                    Margin = new Thickness(8),
                    Fill = new SolidColorBrush(Colors.LightGray)
                };
                RedResponsePanel.Children.Add(ellipse);
            }

            RedCheckButton.Click += delegate(object sender, RoutedEventArgs args)
            {
                for (var i = 0; i < RedResponsePanel.Children.Count; i++)
                {
                    var child = RedResponsePanel.Children[i] as Ellipse;
                    if (child?.Fill.ToString() == "#FFD3D3D3")
                    {
                        Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $" CheckResponseDialog.xaml => {i}");
                    }
                }
            };

            for (var i = 0; i < 60; i++)
            {
                var ellipse = new Ellipse
                {
                    Width = 25,
                    Height = 25,
                    Margin = new Thickness(8),
                    Fill = new SolidColorBrush(Colors.LightGray)
                };
                BlueResponsePanel.Children.Add(ellipse);
            }

            BlueCheckButton.Click += delegate(object sender, RoutedEventArgs args) { };
        }

        private void CheckResponseDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            MouseDown += delegate { DragMove(); };
        }
    }
}