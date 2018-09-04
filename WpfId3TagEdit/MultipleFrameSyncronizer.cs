using ID3TagEditLib;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.Linq;
using System.ComponentModel;
using System;

namespace WpfId3TagEdit
{
    public class MultipleFrameSyncronizer : MultipleValueSyncronizer
    {
        private string frameId;
        private Dictionary<EditID3File, EditTextFrame> frames = new Dictionary<EditID3File, EditTextFrame>();
        private List<EditTextFrame> isSettingTextFrames;

        public string FrameId
        {
            get { return frameId; }
            set
            {
                if (value == frameId) return;

                frameId = value;
                OnPropertyChanged("FrameId");
            }
        }

        public MultipleFrameSyncronizer(ObservableCollection<EditID3File> source, string frameId) : base(source)
        {
            FrameId = frameId;
            isSettingTextFrames = new List<EditTextFrame>();
        }

        protected override string GetValue(EditID3File file)
        {
            EditTextFrame frame;

            return frames.TryGetValue(file, out frame) ? frame.Text : string.Empty;
        }

        protected override void SetValue(string value, EditID3File file)
        {
            EditTextFrame frame;

            if (!frames.TryGetValue(file, out frame))
            {
                frame = EditTextFrame.GetFrameFromId(FrameId);

                frames.Add(file, frame);

                file.ID3v2Tag.Add(frame);
            }

            lock (isSettingTextFrames)
            {
                isSettingTextFrames.Add(frame);
            }

            frame.Text = value;

            lock (isSettingTextFrames)
            {
                isSettingTextFrames.Remove(frame);
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
            if (e.PropertyName == "Text" && !isSettingTextFrames.Contains(sender as EditTextFrame)) Update();
        }

        public ReadOnlyDictionary<EditID3File, EditTextFrame> GetFrames()
        {
            return new ReadOnlyDictionary<EditID3File, EditTextFrame>(frames);
        }
    }
}
