using FolderFile;
using ID3TagEditLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace WpfId3TagEdit
{
    public class ViewModel : INotifyPropertyChanged
    {
        private EditID3File currentFile;
        private Folder folder;

        public MultipleTitleSyncronizer Title { get; private set; }

        public MultipleArtistSyncronizer Artist { get; private set; }

        public MultipleAlbumSyncronizer Album { get; private set; }

        public MultipleYearSyncronizer Year { get; private set; }

        public MultipleTrackNumberSyncronizer TrackNumber { get; private set; }

        public int TagsCount { get { return currentFile?.ID3v2Tag.Count ?? 0; } }

        public int TagsDiffCount
        {
            get
            {
                EditFrame[] frames = currentFile?.ID3v2Tag?.ToArray() ?? new EditFrame[0];

                return frames.Length - frames.Count(f => f is EditTextFrame);
            }
        }

        public FrameSyncronizerCollection CurrentFrames { get; private set; }

        public ObservableCollection<EditID3File> SelectedFiles { get; private set; }

        public List<EditID3File> Files { get { return folder.GetFiles().Select(f => new EditID3File(f)).ToList(); } }

        public Folder FilesFolder
        {
            get { return folder; }
            set
            {
                if (folder == value) return;

                folder = value;

                OnPropertyChanged("FilesFolder");
                OnPropertyChanged("Files");
            }
        }

        public ViewModel()
        {
            string myMusicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            myMusicPath = @"H:\Musik";

            folder = new Folder(myMusicPath, SubfolderType.This);

            SelectedFiles = new ObservableCollection<EditID3File>();
            SelectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;

            CurrentFrames = new FrameSyncronizerCollection(SelectedFiles);

            Title = new MultipleTitleSyncronizer(SelectedFiles);
            Artist = new MultipleArtistSyncronizer(SelectedFiles);
            Album = new MultipleAlbumSyncronizer(SelectedFiles);
            Year = new MultipleYearSyncronizer(SelectedFiles);
            TrackNumber = new MultipleTrackNumberSyncronizer(SelectedFiles);
        }

        private void SelectedFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (currentFile == SelectedFiles.FirstOrDefault()) return;

            currentFile = SelectedFiles.FirstOrDefault();

            OnPropertyChanged("TagsCount");
            OnPropertyChanged("TagsDiffCount");
        }

        public void UpdateFilesList()
        {
            folder.RefreshFolderAndFiles();
            OnPropertyChanged("FilesList");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
