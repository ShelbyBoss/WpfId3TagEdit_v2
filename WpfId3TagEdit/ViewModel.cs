using FolderFile;
using ID3TagEditLib;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace WpfId3TagEdit
{
    public class ViewModel : INotifyPropertyChanged
    {
        private Tuple<EditID3File, IsUnsynchronizedDetector> currentFile;
        private Folder folder;
        private Tuple<EditID3File, IsUnsynchronizedDetector>[] files;

        public MultipleTitleSyncronizer Title { get; private set; }

        public MultipleArtistSyncronizer Artist { get; private set; }

        public MultipleAlbumSyncronizer Album { get; private set; }

        public MultipleYearSyncronizer Year { get; private set; }

        public MultipleTrackNumberSyncronizer TrackNumber { get; private set; }

        public Tuple<EditID3File, IsUnsynchronizedDetector> CurrentFile
        {
            get { return currentFile; }
            set
            {
                if (value == currentFile) return;

                currentFile = value;

                OnPropertyChanged(nameof(CurrentFile));
                OnPropertyChanged(nameof(TagsCount));
                OnPropertyChanged(nameof(TagsDiffCount));
            }
        }

        public int TagsCount { get { return CurrentFile?.Item1?.V2Tag.Count ?? 0; } }

        public int TagsDiffCount
        {
            get
            {
                EditFrame[] frames = CurrentFile?.Item1?.V2Tag?.ToArray() ?? new EditFrame[0];

                return frames.Length - frames.Count(f => f is EditTextFrame);
            }
        }

        public FrameSyncronizerCollection CurrentFrames { get; private set; }

        public ObservableCollection<EditID3File> SelectedFiles { get; private set; }

        public Tuple<EditID3File, IsUnsynchronizedDetector>[] Files
        {
            get { return files; }
            set
            {
                if (value == files) return;

                files = value;
                OnPropertyChanged(nameof(Files));
            }
        }

        public Folder FilesFolder
        {
            get { return folder; }
            set
            {
                if (folder == value) return;

                folder = value;

                OnPropertyChanged(nameof(FilesFolder));

                Files = folder.Files.Select(GetFileTuple).ToArray();
            }
        }

        public ViewModel()
        {
            string myMusicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            //myMusicPath = @"H:\Musik";

            FilesFolder = new Folder(myMusicPath, SubfolderType.This);

            SelectedFiles = new ObservableCollection<EditID3File>();

            CurrentFrames = new FrameSyncronizerCollection(SelectedFiles);

            Title = new MultipleTitleSyncronizer(SelectedFiles);
            Artist = new MultipleArtistSyncronizer(SelectedFiles);
            Album = new MultipleAlbumSyncronizer(SelectedFiles);
            Year = new MultipleYearSyncronizer(SelectedFiles);
            TrackNumber = new MultipleTrackNumberSyncronizer(SelectedFiles);
        }

        public void UpdateFilesList()
        {
            Files = folder.Refresh().Select(GetFileTuple).ToArray();
        }

        private Tuple<EditID3File, IsUnsynchronizedDetector> GetFileTuple(FileInfo source)
        {
            EditID3File file = new EditID3File(source);
            IsUnsynchronizedDetector detector = new IsUnsynchronizedDetector(file);

            return (file, detector).ToTuple();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
