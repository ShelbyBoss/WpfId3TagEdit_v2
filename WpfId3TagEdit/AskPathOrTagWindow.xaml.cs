using ID3TagEditLib;
using System.Windows;
using System.Windows.Input;

namespace WpfId3TagEdit
{
    public partial class AskPathOrTagWindow : Window
    {
        private string result;

        public AskPathOrTagWindow()
        {
            InitializeComponent();
        }

        public string Ask(ArtistTitle kind, string path, string tag)
        {
            tblQuestion.Text += kind.ToString();
            tblPath.Text += path;
            tblTag.Text += tag;

            ShowDialog();

            return result;
        }

        private void Path_Click(object sender, RoutedEventArgs e)
        {
            result = tblPath.Text;
            Close();
        }

        private void Tag_Click(object sender, RoutedEventArgs e)
        {
            result = tblTag.Text;
            Close();
        }

        private void Other_Click(object sender, RoutedEventArgs e)
        {
            result = tbxOther.Text;
            Close();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                e.Handled = true;
                result = tblPath.Text;
                Close();
            }
            else if (e.Key == Key.S)
            {
                e.Handled = true;
                result = tblTag.Text;
                Close();
            }
            else if (e.Key == Key.D)
            {
                e.Handled = true;
                result = tbxOther.Text;
                Close();
            }
        }
    }
}
