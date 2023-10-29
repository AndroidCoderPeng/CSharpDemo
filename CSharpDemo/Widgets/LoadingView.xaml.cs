using System.Windows;
using System.Windows.Controls;

namespace CSharpDemo.Widgets
{
    public partial class LoadingView : UserControl
    {
        public static readonly DependencyProperty LoadingTextProperty = DependencyProperty.Register(
            nameof(LoadingText), typeof(string), typeof(LoadingView),
            new PropertyMetadata("Loading...")
        );

        //声明属性        
        public string LoadingText
        {
            get => (string)GetValue(LoadingTextProperty);
            set => SetValue(LoadingTextProperty, value);
        }

        public LoadingView()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}