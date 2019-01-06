using ID3TagEditLib;
using System.Collections.ObjectModel;

namespace WpfId3TagEdit
{
    public class MultipleYearSyncronizer : MultiplePropertySyncronizerSyncronizer
    {
        public MultipleYearSyncronizer(ObservableCollection<EditID3File> source) : base(source)
        {
        }

        protected override string GetPropertyName()
        {
            EditID3File file;
            return nameof(file.Year);
        }

        protected override PropertySynchronizer GetPropertySynchornizer(EditID3File file)
        {
            return file.Year;
        }
    }
}
