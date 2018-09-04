using ID3TagEditLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfId3TagEdit
{
    public partial class MainWindow : Window
    {
        private ViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            Width = 1200;
            Height = 800;

            DataContext = viewModel = new ViewModel();
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            foreach (EditID3File editFile in viewModel.SelectedFiles)
            {
                try
                {
                    editFile.FilterID3v2Tags(AskRemoveFrame);
                    editFile.Save();
                }
                catch { }
            }
        }

        private void PathToTag_Click(object sender, RoutedEventArgs e)
        {
            foreach (EditID3File editFile in viewModel.SelectedFiles)
            {
                try
                {
                    editFile.FileNameToID3Tag();
                    editFile.Save();
                }
                catch { }
            }
        }

        private void TagToPath_Click(object sender, RoutedEventArgs e)
        {
            string[] values = viewModel.CurrentFrames.Select(f => f.Value).ToArray();

            foreach (EditID3File editFile in viewModel.SelectedFiles)
            {
                try
                {
                    editFile.Save();

                    if (!editFile.IsSyncronized())
                    {
                        lbxFiles.SelectedItem = editFile;
                        break;
                    }
                }
                catch { }
            }
        }

        private void TagVsPath_Click(object sender, RoutedEventArgs e)
        {
            foreach (EditID3File editFile in viewModel.SelectedFiles)
            {
                try
                {
                    editFile.CompareID3v2TagWithPath(false, AskPathOrTag);
                    editFile.ID3v2ToID3v1();
                    editFile.Save();
                }
                catch { }
            }
        }

        private void AskForEveryTitle_Click(object sender, RoutedEventArgs e)
        {
            var frameIdResult = new GetTextWindow().Get("Frame ID:");

            if (frameIdResult.Item1 != MessageBoxResult.Yes) return;

            string frameId = frameIdResult.Item2;

            foreach (EditID3File editFile in viewModel.SelectedFiles)
            {
                try
                {
                    var result = new GetTextWindow().Get(editFile.Source.ToString() + ":" + frameId + ":");

                    if (result.Item1 == MessageBoxResult.Cancel) return;
                    else if (result.Item1 == MessageBoxResult.Yes)
                    {
                        editFile.ChangeOrCreateTextFrame(frameId, result.Item2);
                        editFile.ID3v2ToID3v1();
                        editFile.Save();
                    }
                }
                catch { }
            }
        }

        private PathTagClass AskPathOrTag(ArtistTitle kind, string path, string tag)
        {
            return new AskPathOrTagWindow().Ask(kind, path, tag);
        }

        private bool AskRemoveFrame(string id, string text)
        {
            return MessageBox.Show(string.Format("Remove Frame?\n ID: {0}\nText: \"{1}\"", id, text),
                "Remove?", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }


        private void lbxFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (EditID3File editFile in (IEnumerable)e.RemovedItems ?? Enumerable.Empty<EditID3File>())
            {
                viewModel.SelectedFiles.Remove(editFile);
            }

            foreach (EditID3File editFile in (IEnumerable)e.AddedItems ?? Enumerable.Empty<EditID3File>())
            {
                viewModel.SelectedFiles.Add(editFile);
            }
        }

        private void IDv2ToIDv1_Click(object sender, RoutedEventArgs e)
        {
            foreach (EditID3File editFile in viewModel.SelectedFiles)
            {
                try
                {
                    editFile.ID3v2ToID3v1();
                    editFile.Save();
                }
                catch { }
            }
        }

        private void IDv1ToIDv2_Click(object sender, RoutedEventArgs e)
        {
            foreach (EditID3File editFile in viewModel.SelectedFiles)
            {
                try
                {
                    editFile.ID3v1ToID3v2();
                    editFile.Save();
                }
                catch { }
            }
        }

        private void RemoveNonTextFrames_Click(object sender, RoutedEventArgs e)
        {
            foreach (EditID3File editFile in viewModel.SelectedFiles)
            {
                try
                {
                    editFile.RemoveNonTextFrames();
                    editFile.Save();
                }
                catch { }
            }
        }

        private void RemoveEmptyTextFrames_Click(object sender, RoutedEventArgs e)
        {
            foreach (EditID3File editFile in viewModel.SelectedFiles)
            {
                try
                {
                    IEnumerable<EditTextFrame> emptyFrames = editFile.ID3v2Tag.
                        OfType<EditTextFrame>().Where(f => string.IsNullOrWhiteSpace(f.Text)).ToArray();

                    foreach (EditTextFrame frame in emptyFrames)
                    {
                        editFile.ID3v2Tag.Remove(frame);
                    }

                    editFile.Save();
                }
                catch { }
            }
        }

        private void BtnAddFrame_Click(object sender, RoutedEventArgs e)
        {
            var frameIdResult = new GetTextWindow().Get("Frame ID:");

            if (frameIdResult.Item1 == MessageBoxResult.Yes)
            {
                MultipleFrameSyncronizer syncronizer = new MultipleFrameSyncronizer(viewModel.SelectedFiles, frameIdResult.Item2);

                viewModel.CurrentFrames.Add(syncronizer);
            }
        }
    }
}
