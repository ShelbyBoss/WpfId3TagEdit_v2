using System;
using System.Collections.ObjectModel;
using ID3TagEditLib;
using System.ComponentModel;

namespace WpfId3TagEdit
{
    public class MultipleYearSyncronizer : MultipleValueSyncronizer
    {
        public MultipleYearSyncronizer(ObservableCollection<EditID3File> source) : base(source)
        {
        }

        protected override string GetValue(EditID3File file)
        {
            return file.Year.Value;
        }

        protected override void SetValue(string value, EditID3File file)
        {
            file.Year.Value = value;
        }

        protected override void Subscribe(EditID3File file)
        {
            file.Year.PropertyChanged += OnPropertyChanged;
        }

        protected override void Unsubscribe(EditID3File file)
        {
            file.Year.PropertyChanged -= OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value") Update();
        }
    }
}
