using ID3TagEditLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WpfId3TagEdit
{
    public class MultipleFrameSyncronizer : MultipleValueSyncronizer
    {
        private string frameId;
        private Dictionary<EditID3File, EditTextFrame> frames = new Dictionary<EditID3File, EditTextFrame>();
        private List<EditTextFrame> settingTextFrames;

        public string FrameId
        {
            get { return frameId; }
            set
            {
                if (value == frameId) return;

                frameId = value;
                OnPropertyChanged(nameof(FrameId));
            }
        }

        public MultipleFrameSyncronizer(ObservableCollection<EditID3File> source, string frameId) : base(source)
        {
            FrameId = frameId;
            settingTextFrames = new List<EditTextFrame>();
        }

        protected override string GetValue(EditID3File file)
        {
            EditTextFrame frame;

            return frames.TryGetValue(file, out frame) ? frame.Text : string.Empty;
        }

        protected override bool GetIsSync(EditID3File file)
        {
            return true;
        }

        protected override void SetValue(string value, EditID3File file)
        {
            EditTextFrame frame;

            if (!frames.TryGetValue(file, out frame))
            {
                frame = EditTextFrame.GetFrameFromId(FrameId);

                frames.Add(file, frame);

                file.V2Tag.Add(frame);
            }

            lock (settingTextFrames)
            {
                settingTextFrames.Add(frame);
            }

            frame.Text = value;

            lock (settingTextFrames)
            {
                settingTextFrames.Remove(frame);
            }
        }

        protected override void Subscribe(EditID3File file)
        {
        }

        protected override void Unsubscribe(EditID3File file)
        {
            EditTextFrame frame;

            if (frames.TryGetValue(file, out frame))
            {
                frame.PropertyChanged -= Frame_PropertyChanged;
                frames.Remove(file);
            }
        }

        public void AddFrame(EditTextFrame frame, EditID3File file)
        {
            if (frames.ContainsKey(file)) return;

            frames.Add(file, frame);
            frame.PropertyChanged += Frame_PropertyChanged;

            Update();
        }

        public bool RemoveFrame(EditID3File file)
        {
            return frames.Remove(file);
        }

        public bool RemoveFrame(EditTextFrame frame)
        {
            EditID3File file = null;

            foreach (KeyValuePair<EditID3File, EditTextFrame> pair in frames)
            {
                if (pair.Value != frame) continue;

                file = pair.Key;
                break;
            }

            if (file == null) return false;

            frame.PropertyChanged -= Frame_PropertyChanged;

            return frames.Remove(file);
        }

        private void Frame_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            EditTextFrame frame = sender as EditTextFrame;

            if (e.PropertyName == nameof(frame.Text) && !settingTextFrames.Contains(frame)) Update();
        }

        public ReadOnlyDictionary<EditID3File, EditTextFrame> GetFrames()
        {
            return new ReadOnlyDictionary<EditID3File, EditTextFrame>(frames);
        }
    }
}
