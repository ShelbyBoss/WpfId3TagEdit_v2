using ID3TagEditLib;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace WpfId3TagEdit
{
  public  class  EditTextFrameCollection : ObservableCollection<EditTextFrame>
    {
        private ObservableCollection<EditFrame> parent;

        public EditTextFrameCollection(ObservableCollection<EditFrame> parent)
        {
            this.parent = parent;
            parent.CollectionChanged += Parent_CollectionChanged;

            foreach (EditTextFrame frame in parent.OfType<EditTextFrame>())
            {
                Add(frame);
            }
        }

        private void Parent_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (EditTextFrame frame in e.NewItems?.OfType<EditTextFrame>() ?? Enumerable.Empty<EditTextFrame>())
            {
                Add(frame);
            }

            foreach (EditTextFrame frame in e.OldItems?.OfType<EditTextFrame>() ?? Enumerable.Empty<EditTextFrame>())
            {
                Remove(frame);
            }
        }
    }
}
