using ID3TagEditLib;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace WpfId3TagEdit
{
    public class FrameSyncronizerCollection : ObservableCollection<MultipleFrameSyncronizer>
    {
        private ObservableCollection<EditID3File> parent;

        public FrameSyncronizerCollection(ObservableCollection<EditID3File> parent)
        {
            this.parent = parent;

            foreach (EditID3File file in parent)
            {
                Add(file);
            }

            parent.CollectionChanged += Parent_CollectionChanged;
        }

        private void Parent_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (EditID3File file in (IEnumerable)e.NewItems ?? Enumerable.Empty<EditID3File>())
            {
                Add(file);
            }

            foreach (EditID3File file in (IEnumerable)e.OldItems ?? Enumerable.Empty<EditID3File>())
            {
                Remove(file);
            }
        }

        private void Add(EditID3File file)
        {
            foreach (var frameIdGroup in file.ID3v2Tag.OfType<EditTextFrame>().GroupBy(f => f.FrameId))
            {
                Add(frameIdGroup, file);
            }

            file.ID3v2Tag.CollectionChanged += ID3v2Tag_CollectionChanged;
        }

        private void Remove(EditID3File file)
        {
            foreach (MultipleFrameSyncronizer syncronizer in this.ToArray())
            {
                if (syncronizer.RemoveFrame(file) && !syncronizer.GetFrames().Any()) Remove(syncronizer);
            }
        }

        private void ID3v2Tag_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            EditID3File file = parent.FirstOrDefault(f => f.ID3v2Tag == sender);

            if (file != null)
            {
                foreach (EditTextFrame frame in e.NewItems?.OfType<EditTextFrame>() ?? Enumerable.Empty<EditTextFrame>())
                {
                    MultipleFrameSyncronizer syncornizer = this.FirstOrDefault
                        (s => s.FrameId == frame.FrameId && s.GetFrames().Any(f => f.Key == file));

                    if (syncornizer == null)
                    {
                        syncornizer = new MultipleFrameSyncronizer(parent, frame.FrameId);
                        Add(syncornizer);
                    }

                    syncornizer.AddFrame(frame, file);
                }
            }

            foreach (EditTextFrame frame in e.OldItems?.OfType<EditTextFrame>() ?? Enumerable.Empty<EditTextFrame>())
            {
                foreach (MultipleFrameSyncronizer syncronizer in this.ToArray())
                {
                    if (syncronizer.RemoveFrame(frame)) break;
                    if (!syncronizer.GetFrames().Any()) Remove(syncronizer);
                }
            }
        }

        private void Add(IGrouping<string, EditTextFrame> frameIdGroup, EditID3File file)
        {
            List<EditTextFrame> frames = frameIdGroup.ToList();
            List<MultipleFrameSyncronizer> syncronizers = this.Where(c => c.FrameId == frameIdGroup.Key).ToList();

            foreach (EditTextFrame frame in frames.ToArray())
            {
                MultipleFrameSyncronizer syncronizer = syncronizers.FirstOrDefault(c => c.Value == frame.Text);

                if (syncronizer == null) continue;

                syncronizer.AddFrame(frame, file);

                syncronizers.Remove(syncronizer);
                frames.Remove(frame);
            }

            foreach (EditTextFrame frame in frames)
            {
                MultipleFrameSyncronizer syncronizer = syncronizers.FirstOrDefault();

                if (syncronizer == null)
                {
                    syncronizer = new MultipleFrameSyncronizer(parent, frame.FrameId);
                    Add(syncronizer);
                }

                syncronizer.AddFrame(frame, file);
                syncronizers.Remove(syncronizer);
            }
        }

        protected override void InsertItem(int index, MultipleFrameSyncronizer item)
        {
            item.Files = parent;

            base.InsertItem(index, item);
        }

        protected override void ClearItems()
        {
            foreach (EditID3File file in parent)
            {
                file.ID3v2Tag.Clear();
            }

            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            MultipleFrameSyncronizer syncronizer = this[index];

            foreach (KeyValuePair<EditID3File, EditTextFrame> pair in syncronizer.GetFrames().ToArray())
            {
                pair.Key.ID3v2Tag.Remove(pair.Value);
            }

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, MultipleFrameSyncronizer item)
        {
            MultipleFrameSyncronizer replaceSyncronizer = this[index];

            foreach (KeyValuePair<EditID3File, EditTextFrame> pair in replaceSyncronizer.GetFrames())
            {
                pair.Key.ID3v2Tag.Remove(pair.Value);
            }

            base.SetItem(index, item);
        }
    }
}
