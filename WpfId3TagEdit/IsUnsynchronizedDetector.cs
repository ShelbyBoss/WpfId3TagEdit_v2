using ID3TagEditLib;
using System.Collections.Generic;
using System.ComponentModel;

namespace WpfId3TagEdit
{
    public class IsUnsynchronizedDetector : ISync, INotifyPropertyChanged
    {
        private readonly Dictionary<EditID3File, (ISync album, ISync artist, ISync title, ISync trackNumber, ISync year, ISync fileName)> synchonizers;

        private bool isSync;
        private EditID3File source;

        public bool IsSync
        {
            get { return isSync; }
            set
            {
                if (value == isSync) return;

                isSync = value;
                OnPropertyChanged(nameof(IsSync));
            }
        }

        public EditID3File Source
        {
            get { return source; }
            set
            {
                if (value == source) return;

                if (source != null)
                {
                    source.PropertyChanged -= Source_PropertyChanged;
                    source.Album.PropertyChanged -= Source_Property_PropertyChanged;
                    source.Artist.PropertyChanged -= Source_Property_PropertyChanged;
                    source.Title.PropertyChanged -= Source_Property_PropertyChanged;
                    source.TrackNumber.PropertyChanged -= Source_Property_PropertyChanged;
                    source.Year.PropertyChanged -= Source_Property_PropertyChanged;
                    source.FileName.PropertyChanged -= Source_Property_PropertyChanged;

                    synchonizers.Remove(source);
                }

                if (value != null)
                {
                    value.PropertyChanged += Source_PropertyChanged;

                    value.Album.PropertyChanged += Source_Property_PropertyChanged;
                    value.Artist.PropertyChanged += Source_Property_PropertyChanged;
                    value.Title.PropertyChanged += Source_Property_PropertyChanged;
                    value.TrackNumber.PropertyChanged += Source_Property_PropertyChanged;
                    value.Year.PropertyChanged += Source_Property_PropertyChanged;
                    value.FileName.PropertyChanged += Source_Property_PropertyChanged;

                    synchonizers.Add(value, (value.Album, value.Artist, value.Title, value.TrackNumber, value.Year, value.FileName));
                }

                source = value;
                OnPropertyChanged(nameof(Source));

                IsSync = source?.IsSyncronized() ?? true;
            }
        }

        public IsUnsynchronizedDetector()
        {
            synchonizers = new Dictionary<EditID3File, (ISync album, ISync artist, ISync title, ISync trackNumber, ISync year, ISync fileName)>();

            IsSync = true;
        }

        public IsUnsynchronizedDetector(EditID3File source) : this()
        {
            Source = source;
        }

        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            EditID3File file = (EditID3File)sender;
            var tuple = synchonizers[file];

            switch (e.PropertyName)
            {
                case nameof(file.Album):
                    tuple.album.PropertyChanged -= Source_Property_PropertyChanged;
                    tuple.album = file.Album;
                    tuple.album.PropertyChanged += Source_Property_PropertyChanged;
                    break;

                case nameof(file.Artist):
                    tuple.artist.PropertyChanged -= Source_Property_PropertyChanged;
                    tuple.artist = file.Artist;
                    tuple.artist.PropertyChanged += Source_Property_PropertyChanged;
                    break;

                case nameof(file.Title):
                    tuple.title.PropertyChanged -= Source_Property_PropertyChanged;
                    tuple.title = file.Title;
                    tuple.title.PropertyChanged += Source_Property_PropertyChanged;
                    break;

                case nameof(file.TrackNumber):
                    tuple.trackNumber.PropertyChanged -= Source_Property_PropertyChanged;
                    tuple.trackNumber = file.TrackNumber;
                    tuple.trackNumber.PropertyChanged += Source_Property_PropertyChanged;
                    break;

                case nameof(file.Year):
                    tuple.year.PropertyChanged -= Source_Property_PropertyChanged;
                    tuple.year = file.Year;
                    tuple.year.PropertyChanged += Source_Property_PropertyChanged;
                    break;

                case nameof(file.FileName):
                    tuple.fileName.PropertyChanged -= Source_Property_PropertyChanged;
                    tuple.fileName = file.FileName;
                    tuple.fileName.PropertyChanged += Source_Property_PropertyChanged;
                    break;

                default:
                    return;
            }

            synchonizers[file] = tuple;

            IsSync = source?.IsSyncronized() ?? true;
        }

        private void Source_Property_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ISync sync = (ISync)sender;

            if (e.PropertyName == nameof(sync.IsSync)) IsSync = Source?.IsSyncronized() ?? true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
