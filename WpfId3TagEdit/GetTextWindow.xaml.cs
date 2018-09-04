using System;
using System.Windows;
using System.Windows.Input;

namespace WpfId3TagEdit
{
    public partial class GetTextWindow : Window
    {
        private MessageBoxResult result;

        public GetTextWindow()
        {
            InitializeComponent();
        }

        public Tuple<MessageBoxResult, string> Get(string question)
        {
            tblText.Text = question;
            tbxText.Focus();
            ShowDialog();

            return new Tuple<MessageBoxResult, string>(result, tbxText.Text);
        }

        private void tbxText_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                result = MessageBoxResult.Yes;
                Close();
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                result = MessageBoxResult.Cancel;
                Close();
            }
            else if (e.Key == Key.Tab)
            {
                e.Handled = true;
                result = MessageBoxResult.No;
                Close();
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Yes;
            Close();
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.No;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Cancel;
            Close();
        }
    }
}
