using ID3TagEditLib;
using System.Windows;
using System.Windows.Input;

namespace WpfId3TagEdit
{
    public partial class AskPathOrTagWindow : Window
    {
        PathTagClass result;

        public AskPathOrTagWindow()
        {
            InitializeComponent();
        }

        public PathTagClass Ask(ArtistTitle kind, string path, string tag)
        {
            tblQuestion.Text += kind.ToString();
            tblPath.Text += path;
            tblTag.Text += tag;

            ShowDialog();

            return result;
        }

        private void Path_Click(object sender, RoutedEventArgs e)
        {
            result = PathTagClass.GetPath();
            Close();
        }

        private void Tag_Click(object sender, RoutedEventArgs e)
        {
            result = PathTagClass.GetTag();
            Close();
        }

        private void Other_Click(object sender, RoutedEventArgs e)
        {
            result = PathTagClass.GetOther(tbxOther.Text);
            Close();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                e.Handled = true;
                result = PathTagClass.GetPath();
                Close();
            }
            else if (e.Key == Key.S)
            {
                e.Handled = true;
                result = PathTagClass.GetTag();
                Close();
            }
            else if (e.Key == Key.D)
            {
                e.Handled = true;
                result = PathTagClass.GetOther(tbxOther.Text);
                Close();
            }
        }
    }
}
