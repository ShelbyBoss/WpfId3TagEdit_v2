using ID3TagEditLib;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace WpfId3TagEdit
{
    public class FrameSyncronizerCollection : ObservableCollection<MultipleFrameSyncronizer>
    {
        private readonly Dictionary<EditID3File, EditID3v2Tag> tags;

        private ObservableCollection<EditID3File> parent;

        public FrameSyncronizerCollection(ObservableCollection<EditID3File> parent)
        {
            tags = new Dictionary<EditID3File, EditID3v2Tag>();

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
            foreach (var frameIdGroup in file.V2Tag.OfType<EditTextFrame>().GroupBy(f => f.FrameId))
            {
                Add(frameIdGroup, file);
            }

            file.PropertyChanged += File_PropertyChanged;
            file.V2Tag.CollectionChanged += ID3v2Tag_CollectionChanged;

            if (tags.ContainsKey(file)) tags[file] = file.V2Tag;
            else tags.Add(file, file.V2Tag);
        }

        private void Remove(EditID3File file)
        {
            foreach (MultipleFrameSyncronizer syncronizer in this.ToArray())
            {
                if (syncronizer.RemoveFrame(file) && !syncronizer.GetFrames().Any()) Remove(syncronizer);
            }

            file.PropertyChanged -= File_PropertyChanged;

            EditID3v2Tag tag;
            if (tags.TryGetValue(file, out tag)) tag.CollectionChanged -= ID3v2Tag_CollectionChanged;
        }

        private void File_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            EditID3File file = (EditID3File)sender;

            if (e.PropertyName == nameof(file.V2Tag))
            {
                Remove(file);
                Add(file);
            }
        }

        private void ID3v2Tag_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            EditID3File file = parent.FirstOrDefault(f => f.V2Tag == sender);

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
                    if (syncronizer.RemoveFrame(frame))
                    {
                        if (!syncronizer.GetFrames().Any()) Remove(syncronizer);

                        break;
                    }
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
                file.V2Tag.Clear();
            }

            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            MultipleFrameSyncronizer syncronizer = this[index];

            base.RemoveItem(index);

            foreach (KeyValuePair<EditID3File, EditTextFrame> pair in syncronizer.GetFrames().ToArray())
            {
                pair.Key.V2Tag.Remove(pair.Value);
            }
        }

        protected override void SetItem(int index, MultipleFrameSyncronizer item)
        {
            MultipleFrameSyncronizer replaceSyncronizer = this[index];

            foreach (KeyValuePair<EditID3File, EditTextFrame> pair in replaceSyncronizer.GetFrames())
            {
                pair.Key.V2Tag.Remove(pair.Value);
            }

            base.SetItem(index, item);
        }
    }
}
