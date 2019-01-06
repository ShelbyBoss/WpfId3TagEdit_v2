using ID3TagLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ID3TagEditLib
{
    public class EditID3v2Tag : ObservableCollection<EditFrame>
    {
        internal ID3v2Tag Parent { get; private set; }

        public EditID3v2Tag(ID3v2Tag parent)
        {
            this.Parent = parent;

            for (int i = 0; i < parent.Frames.Count; i++)
            {
                base.InsertItem(i, parent.Frames[i]);
            }
        }

        protected override void ClearItems()
        {
            base.ClearItems();
        }

        protected override void InsertItem(int index, EditFrame item)
        {
            Parent.Frames.Insert(index, item);
            base.InsertItem(index, item);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            Frame frame = Parent.Frames[oldIndex];
            Parent.Frames.RemoveAt(oldIndex);
            Parent.Frames.Insert(newIndex, frame);

            base.MoveItem(oldIndex, newIndex);
        }

        protected override void RemoveItem(int index)
        {
            Parent.Frames.RemoveAt(index);

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, EditFrame item)
        {
            Parent.Frames[index] = item;

            base.SetItem(index, item);
        }

        public IEnumerable<EditTextFrame> GetTextFrames()
        {
            return this.OfType<EditTextFrame>();
        }
    }
}
