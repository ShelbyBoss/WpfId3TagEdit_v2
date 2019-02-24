using ID3TagEditLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfId3TagEdit
{
    public partial class MainWindow : Window
    {
        private ViewModel viewModel;
        private System.Windows.Forms.OpenFileDialog ofd;

        public MainWindow()
        {
            InitializeComponent();

            Width = 1200;
            Height = 800;

            DataContext = viewModel = new ViewModel();

            ofd = new System.Windows.Forms.OpenFileDialog()
            {
                InitialDirectory = viewModel.FilesFolder.FullName,
                RestoreDirectory = true
            };
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
                catch (Exception exc)
                {
                    if (MessageBox.Show(exc.ToString(), "FilterID3V2Frames", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) break;
                }
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
                catch (Exception exc)
                {
                    if (MessageBox.Show(exc.ToString(), "PathToTag", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) break;
                }
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
                    editFile.FileName.FileName = editFile.FileName.TagFileName;

                    //if (!tuple.IsSyncronized())
                    //{
                    //    lbxFiles.SelectedItem = tuple;
                    //    break;
                    //}
                }
                catch (Exception exc)
                {
                    if (MessageBox.Show(exc.ToString(), "TagToPath", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) break;
                }
            }
        }

        private void TagVsPath_Click(object sender, RoutedEventArgs e)
        {
            foreach (EditID3File editFile in viewModel.SelectedFiles)
            {
                try
                {
                    editFile.CompareID3v2TagWithPath(false, AskPathOrTag);
                    editFile.Save();
                }
                catch (Exception exc)
                {
                    if (MessageBox.Show(exc.ToString(), "TagVsPath", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) break;
                }
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
                        editFile.Save();
                    }
                }
                catch (Exception exc)
                {
                    if (MessageBox.Show(exc.ToString(), "AskForEveryTitle", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) break;
                }
            }
        }

        private string AskPathOrTag(ArtistTitle kind, string path, string tag)
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
            foreach (Tuple<EditID3File, IsUnsynchronizedDetector> tuple in
                (IEnumerable)e.RemovedItems ?? Enumerable.Empty<Tuple<EditID3File, IsUnsynchronizedDetector>>())
            {
                viewModel.SelectedFiles.Remove(tuple.Item1);
            }

            foreach (Tuple<EditID3File, IsUnsynchronizedDetector> tuple in
                (IEnumerable)e.AddedItems ?? Enumerable.Empty<Tuple<EditID3File, IsUnsynchronizedDetector>>())
            {
                viewModel.SelectedFiles.Add(tuple.Item1);
            }
        }

        private void IDv2ToIDv1_Click(object sender, RoutedEventArgs e)
        {
            foreach (EditID3File editFile in viewModel.SelectedFiles)
            {
                try
                {
                    editFile.Album.SetV2TagValue();
                    editFile.Artist.SetV2TagValue();
                    editFile.Title.SetV2TagValue();
                    editFile.TrackNumber.SetV2TagValue();
                    editFile.Year.SetV2TagValue();
                }
                catch (Exception exc)
                {
                    if (MessageBox.Show(exc.ToString(), "IDv2ToIDv1", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) break;
                }
            }
        }

        private void IDv1ToIDv2_Click(object sender, RoutedEventArgs e)
        {
            foreach (EditID3File editFile in viewModel.SelectedFiles)
            {
                try
                {
                    editFile.Album.CreateV2FramesAndSetV1Value();
                    editFile.Artist.CreateV2FramesAndSetV1Value();
                    editFile.Title.CreateV2FramesAndSetV1Value();
                    editFile.TrackNumber.CreateV2FramesAndSetV1Value();
                    editFile.Year.CreateV2FramesAndSetV1Value();
                }
                catch (Exception exc)
                {
                    if (MessageBox.Show(exc.ToString(), "IDv1ToIDv2", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) break;
                }
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
                catch (Exception exc)
                {
                    if (MessageBox.Show(exc.ToString(), "RemoveEmptyTextFrames", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) break;
                }
            }
        }

        private void RemoveEmptyTextFrames_Click(object sender, RoutedEventArgs e)
        {
            foreach (EditID3File editFile in viewModel.SelectedFiles)
            {
                try
                {
                    IEnumerable<EditTextFrame> emptyFrames = editFile.V2Tag.
                        OfType<EditTextFrame>().Where(f => string.IsNullOrWhiteSpace(f.Text)).ToArray();

                    foreach (EditTextFrame frame in emptyFrames)
                    {
                        editFile.V2Tag.Remove(frame);
                    }

                    editFile.Save();
                }
                catch (Exception exc)
                {
                    if (MessageBox.Show(exc.ToString(), "RemoveEmptyTextFrames", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) break;
                }
            }
        }

        private void LoadFormOtherFile_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.CurrentFile == null) return;
            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            try
            {
                viewModel.CurrentFile.Item1.LoadID3(ofd.FileName);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString(), "Load ID3 of other file");
            }
        }

        private void BtnAddFrame_Click(object sender, RoutedEventArgs e)
        {
            var frameIdResult = new GetTextWindow().Get("Frame ID:");

            if (frameIdResult.Item1 == MessageBoxResult.Yes)
            {
                foreach (EditID3File editFile in viewModel.SelectedFiles)
                {
                    editFile.V2Tag.Add(EditTextFrame.GetFrameFromId(frameIdResult.Item2));
                }
            }
        }
    }
}
