using System.Collections.ObjectModel;
using ID3TagEditLib;

namespace WpfId3TagEdit
{
    public class MultipleTrackNumberSyncronizer : MultiplePropertySyncronizerSyncronizer
    {
        public MultipleTrackNumberSyncronizer(ObservableCollection<EditID3File> source) : base(source)
        {
        }

        protected override string GetPropertyName()
        {
            EditID3File file;
            return nameof(file.TrackNumber);
        }

        protected override PropertySynchronizer GetPropertySynchornizer(EditID3File file)
        {
            return file.TrackNumber;
        }
    }
}
