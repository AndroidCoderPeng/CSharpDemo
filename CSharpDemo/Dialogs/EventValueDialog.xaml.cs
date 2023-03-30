using System;
using System.Windows;

namespace CSharpDemo.Dialogs
{
    /// <summary>
    /// EventValueDialog.xaml 的交互逻辑
    /// </summary>
    public partial class EventValueDialog : Window
    {
        public event EventHandler<string> ValueChangedEventHandler;

        private void ValueChangedEvent(string value)
        {
            ValueChangedEventHandler?.Invoke(this, value);
        }

        public EventValueDialog()
        {
            InitializeComponent();

            EventButton.Click += delegate
            {
                ValueChangedEvent(EventTextBox.Text);
                Close();
            };
        }
    }
}