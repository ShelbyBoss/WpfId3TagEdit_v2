using System.Collections.ObjectModel;
using ID3TagEditLib;
using System.ComponentModel;

namespace WpfId3TagEdit
{
    public class MultipleArtistSyncronizer : MultipleValueSyncronizer
    {
        public MultipleArtistSyncronizer(ObservableCollection<EditID3File> source) : base(source)
        {
        }

        protected override string GetValue(EditID3File file)
        {
            return file.Artist.Value;
        }

        protected override void SetValue(string value, EditID3File file)
        {
            file.Artist.Value = value;
        }

        protected override void Subscribe(EditID3File file)
        {
            file.Artist.PropertyChanged += OnPropertyChanged;
        }

        protected override void Unsubscribe(EditID3File file)
        {
            file.Artist.PropertyChanged -= OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value") Update();
        }
    }
}
