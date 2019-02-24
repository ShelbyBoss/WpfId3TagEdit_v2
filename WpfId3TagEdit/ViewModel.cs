using FolderFile;
using ID3TagEditLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WpfId3TagEdit
{
    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly EditFileTupleCompare comparer = new EditFileTupleCompare();

        private Tuple<EditID3File, IsUnsynchronizedDetector> currentFile;
        private Folder folder;

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

        public SortedObservableCollection<Tuple<EditID3File, IsUnsynchronizedDetector>> Files { get; private set; }

        public Folder FilesFolder
        {
            get { return folder; }
            set
            {
                if (folder == value) return;

                folder = value;

                OnPropertyChanged(nameof(FilesFolder));

                UpdateFilesList(FilesFolder);
            }
        }

        public ViewModel()
        {
            Files = new SortedObservableCollection<Tuple<EditID3File, IsUnsynchronizedDetector>>(comparer);

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

        public async Task UpdateFilesList(Folder folder)
        {
            FileInfo[] fileInfos = await Task.Run(() => folder?.Refresh()) ?? new FileInfo[0];
            Queue<Tuple<EditID3File, IsUnsynchronizedDetector>> queue = new Queue<Tuple<EditID3File, IsUnsynchronizedDetector>>();

            foreach (var tuple in Files)
            {
                tuple.Item1.FileName.PropertyChanged -= FileName_PropertyChanged;
            }

            Files.Clear();

            Task producer = Task.Run(() =>
            {
                Parallel.For(0, fileInfos.Length, (i, s) =>
                {
                    if (folder != FilesFolder) s.Break();
                    else
                    {
                        Tuple<EditID3File, IsUnsynchronizedDetector> fileTuple = GetFileTuple(fileInfos[i]);

                        lock (queue)
                        {
                            queue.Enqueue(fileTuple);

                            Monitor.Pulse(queue);
                        }
                    }
                });

                lock (queue)
                {
                    Monitor.Pulse(queue);
                }
            });

            while (!producer.IsCompleted && folder == FilesFolder)
            {
                await Task.Run(() => { lock (queue) Monitor.Wait(queue); });

                while (queue.Count > 0 && folder == FilesFolder)
                {
                    var tuple = queue.Dequeue();

                    Files.Add(tuple);
                    tuple.Item1.FileName.PropertyChanged += FileName_PropertyChanged;
                }
            }
        }

        private void FileName_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Files.UpdateCollection();
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
