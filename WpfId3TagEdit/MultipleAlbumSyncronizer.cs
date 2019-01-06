using ID3TagEditLib;
using System.Collections.ObjectModel;

namespace WpfId3TagEdit
{
    public class MultipleAlbumSyncronizer : MultiplePropertySyncronizerSyncronizer
    {
        public MultipleAlbumSyncronizer(ObservableCollection<EditID3File> source) : base(source)
        {
        }

        protected override string GetPropertyName()
        {
            EditID3File file;
            return nameof(file.Album);
        }

        protected override PropertySynchronizer GetPropertySynchornizer(EditID3File file)
        {
            return file.Album;
        }
    }
}
