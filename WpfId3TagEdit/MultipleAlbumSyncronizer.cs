using ID3TagEditLib;
using System.Collections.ObjectModel;
using System;
using System.ComponentModel;

namespace WpfId3TagEdit
{
    public class MultipleAlbumSyncronizer : MultipleValueSyncronizer
    {
        public MultipleAlbumSyncronizer(ObservableCollection<EditID3File> source) : base(source)
        {

        }

        protected override string GetValue(EditID3File file)
        {
            return file.Album.Value;
        }

        protected override void SetValue(string value, EditID3File file)
        {
            file.Album.Value = value;
        }

        protected override void Subscribe(EditID3File file)
        {
            file.Album.PropertyChanged += OnPropertyChanged;
        }

        protected override void Unsubscribe(EditID3File file)
        {
            file.Album.PropertyChanged -= OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value") Update();
        }
    }
}
