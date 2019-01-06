using ID3TagLib;
using System;
using System.ComponentModel;

namespace ID3TagEditLib
{
    public class EditID3v1Tag : INotifyPropertyChanged
    {
        internal ID3v1Tag Parent { get; private set; }

        public string this[string propertyName]
        {
            get { return GetValue(propertyName); }
            set { SetValue(propertyName, value); }
        }

        public string Comment
        {
            get { return Parent.Comment; }
            set
            {
                if (value == Parent.Comment) return;

                Parent.Comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }

        public string Artist
        {
            get { return Parent.Artist; }
            set
            {
                if (value == Parent.Artist) return;

                Parent.Artist = value;
                OnPropertyChanged(nameof(Artist));
            }
        }

        public string Album
        {
            get { return Parent.Album; }
            set
            {
                if (value == Parent.Album) return;

                Parent.Album = value;
                OnPropertyChanged(nameof(Album));
            }
        }

        public string Title
        {
            get { return Parent.Title; }
            set
            {
                if (value == Parent.Title) return;

                Parent.Title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public string TrackNumber
        {
            get { return Parent.TrackNumber; }
            set
            {
                if (value == Parent.TrackNumber) return;

                Parent.TrackNumber = value;
                OnPropertyChanged(nameof(TrackNumber));
            }
        }

        public string Year
        {
            get { return Parent.Year; }
            set
            {
                if (value == Parent.Year) return;

                Parent.Year = value;
                OnPropertyChanged(nameof(Year));
            }
        }

        public EditID3v1Tag(ID3v1Tag parent)
        {
            this.Parent = parent;
        }

        public string GetValue(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Comment):
                    return Comment;

                case nameof(Artist):
                    return Artist;

                case nameof(Album):
                    return Album;

                case nameof(Title):
                    return Title;

                case nameof(TrackNumber):
                    return TrackNumber;

                case nameof(Year):
                    return Year;
            }

            throw new ArgumentException("PropertyName: " + propertyName);
        }

        public void SetValue(string propertyName, string value)
        {
            switch (propertyName)
            {
                case nameof(Comment):
                    Comment = value;
                    break;

                case nameof(Artist):
                    Artist = value;
                    break;

                case nameof(Album):
                    Album = value;
                    break;

                case nameof(Title):
                    Title = value;
                    break;

                case nameof(TrackNumber):
                    TrackNumber = value;
                    break;

                case nameof(Year):
                    Year = value;
                    break;

                default:
                    throw new ArgumentException("PropertyName: " + propertyName);
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
