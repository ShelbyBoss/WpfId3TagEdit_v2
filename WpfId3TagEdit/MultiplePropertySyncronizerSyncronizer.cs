using ID3TagEditLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WpfId3TagEdit
{
    public abstract class MultiplePropertySyncronizerSyncronizer : MultipleValueSyncronizer
    {
        private readonly Dictionary<EditID3File, PropertySynchronizer> synchronizers;

        public MultiplePropertySyncronizerSyncronizer(ObservableCollection<EditID3File> source) : base(source)
        {
            synchronizers = new Dictionary<EditID3File, PropertySynchronizer>();
        }

        protected abstract PropertySynchronizer GetPropertySynchornizer(EditID3File file);

        protected override string GetValue(EditID3File file)
        {
            return GetPropertySynchornizer(file).Value;
        }

        protected override bool GetIsSync(EditID3File file)
        {
            return GetPropertySynchornizer(file).IsSync;
        }

        protected override void SetValue(string value, EditID3File file)
        {
            GetPropertySynchornizer(file).Value = value;
        }

        protected override void Subscribe(EditID3File file)
        {
            file.PropertyChanged += File_PropertyChanged;

            PropertySynchronizer synchronizer = GetPropertySynchornizer(file);

            if (synchronizers.ContainsKey(file)) synchronizers[file] = synchronizer;
            else synchronizers.Add(file, synchronizer);

            synchronizer.PropertyChanged += OnPropertySynchronizer_PropertyChanged;
        }

        protected override void Unsubscribe(EditID3File file)
        {
            file.PropertyChanged -= File_PropertyChanged;

            PropertySynchronizer synchronizer;

            if (!synchronizers.TryGetValue(file, out synchronizer)) return;

            synchronizer.PropertyChanged -= OnPropertySynchronizer_PropertyChanged;
            synchronizers.Remove(file);
        }

        private void File_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            EditID3File file = (EditID3File)sender;

            if (e.PropertyName == GetPropertyName())
            {
                PropertySynchronizer synchronizer = synchronizers[file];

                synchronizer.PropertyChanged -= OnPropertySynchronizer_PropertyChanged;
                synchronizers[file] = synchronizer = GetPropertySynchornizer(file);
                synchronizer.PropertyChanged += OnPropertySynchronizer_PropertyChanged;

                Update();
            }
        }

        protected abstract string GetPropertyName();

        protected void OnPropertySynchronizer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertySynchronizer synchronizer;

            if (e.PropertyName == nameof(synchronizer.Value) || e.PropertyName == nameof(synchronizer.IsSync)) Update();
        }
    }
}
