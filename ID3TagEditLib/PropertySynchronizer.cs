using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ID3TagEditLib
{
    public class PropertySynchronizer : ISync, INotifyPropertyChanged
    {
        private EditID3v1Tag v1Tag;
        private EditID3v2Tag v2Tag;
        private string v1TagPropertyName;
        private string[] v2TagFrameIds;
        private string value;

        public bool IsSync
        {
            get { return value != null; }
        }

        public string Value
        {
            get { return value ?? string.Empty; }
            set
            {
                if (value == this.value) return;

                this.value = value;

                if (value != null)
                {
                    try
                    {
                        v1Tag[v1TagPropertyName] = value;
                    }
                    catch (ArgumentException)
                    {
                        string numericText = string.Empty;

                        foreach(char c in value)
                        {
                            if (!char.IsNumber(c)) break;

                            numericText += c;
                        }

                        try
                        {
                            v1Tag[v1TagPropertyName] = numericText;
                        }
                        catch { }
                    }
                    catch { }

                    foreach (EditTextFrame frame in GetTextFrames())
                    {
                        try
                        {
                            frame.Text = value;
                        }
                        catch { }
                    }
                }

                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(IsSync));
            }
        }

        public PropertySynchronizer(EditID3v1Tag v1Tag, EditID3v2Tag v2Tag, string v1TagPropertyName, params string[] v2TagFrameIds)
        {
            this.v1Tag = v1Tag;
            this.v2Tag = v2Tag;
            this.v1TagPropertyName = v1TagPropertyName;
            this.v2TagFrameIds = v2TagFrameIds;

            v1Tag.PropertyChanged += V1Tag_PropertyChanged;
            v2Tag.CollectionChanged += V2Tag_CollectionChanged;

            foreach (EditTextFrame frame in v2Tag.GetTextFrames())
            {
                if (v2TagFrameIds.Contains(frame.FrameId)) frame.PropertyChanged += Frame_PropertyChanged;
            }

            SetValue();
        }

        private void V1Tag_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == v1TagPropertyName) Value = v1Tag[v1TagPropertyName];
        }

        private void V2Tag_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (EditFrame frame in (IEnumerable)e.NewItems ?? Enumerable.Empty<EditFrame>())
            {
                if (frame is EditTextFrame && v2TagFrameIds.Contains(frame.FrameId))
                {
                    frame.PropertyChanged += Frame_PropertyChanged;
                }
            }

            foreach (EditFrame frame in (IEnumerable)e.OldItems ?? Enumerable.Empty<EditFrame>())
            {
                frame.PropertyChanged -= Frame_PropertyChanged;
            }

            SetValue();
        }

        private void Frame_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            EditTextFrame frame = sender as EditTextFrame;

            if (frame != null && e.PropertyName == nameof(frame.Text)) Value = frame.Text;
        }

        public void SetValue()
        {
            string value = null;

            foreach (string currentValue in GetValues())
            {
                string trimValue = currentValue.Trim();

                if (value == null) value = trimValue;
                else if (value.StartsWith(trimValue)) continue;
                else if (trimValue.StartsWith(value)) value = trimValue;
                else
                {
                    Value = null;
                    return;
                }
            }

            Value = value ?? string.Empty;
        }

        public void SetV1TagValue()
        {
            Value = v1Tag[v1TagPropertyName];
        }

        public void SetV2TagValue()
        {
            if (GetTextFrames().Any()) Value = GetTextFrames().First().Text;
        }

        public void CreateV2TagFrames()
        {
            foreach (string frameId in v2TagFrameIds)
            {
                if (!v2Tag.Any(f => f.FrameId == frameId))
                {
                    EditTextFrame frame = EditTextFrame.GetFrameFromId(frameId);
                    v2Tag.Add(frame);

                    if (IsSync) frame.Text = Value;
                }
            }
        }

        private IEnumerable<string> GetValues()
        {
            string v1TagValue = v1Tag[v1TagPropertyName];

            if (!string.IsNullOrWhiteSpace(v1TagValue)) yield return v1TagValue;

            foreach (EditTextFrame frame in GetTextFrames()) yield return frame.Text;
        }

        private IEnumerable<EditTextFrame> GetTextFrames()
        {
            return v2Tag.GetTextFrames().Where(f => v2TagFrameIds.Contains(f.FrameId));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
