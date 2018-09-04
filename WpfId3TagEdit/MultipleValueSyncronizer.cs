using ID3TagEditLib;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace WpfId3TagEdit
{
    public abstract class MultipleValueSyncronizer : INotifyPropertyChanged
    {
        private const string defaultValue = null;
        private static readonly FontStyle distinctValueTitleStyle = FontStyles.Normal, differentValuesTitleStyle = FontStyles.Italic;

        private ObservableCollection<EditID3File> files;
        private string value;

        public ObservableCollection<EditID3File> Files
        {
            get { return files; }
            set
            {
                if (value == files) return;

                if (files != null)
                {
                    files.CollectionChanged -= Files_CollectionChanged;

                    foreach (EditID3File file in files)
                    {
                        Unsubscribe(file);
                    }
                }

                if (value != null)
                {
                    value.CollectionChanged += Files_CollectionChanged;

                    foreach (EditID3File file in value)
                    {
                        Subscribe(file);
                    }
                }

                files = value;
                OnPropertyChanged("Files");
            }
        }

        public string Value
        {
            get { return value; }
            set
            {
                if (value == this.value) return;

                this.value = value;
                IsDistinctValue = true;

                foreach (EditID3File file in Files ?? Enumerable.Empty<EditID3File>())
                {
                    SetValue(value, file);
                    file.Save();
                }

                OnPropertyChanged("Value");
                OnPropertyChanged("TitleStyle");
            }
        }

        public bool IsDistinctValue { get; private set; }

        public FontStyle TitleStyle { get { return IsDistinctValue ? distinctValueTitleStyle : differentValuesTitleStyle; } }

        public MultipleValueSyncronizer()
        {
            Update();
        }

        public MultipleValueSyncronizer(ObservableCollection<EditID3File> source)
        {
            Files = source;

            Update();

            if (Files != null) Files.CollectionChanged += Files_CollectionChanged;
        }

        private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (EditID3File file in (IEnumerable)e.NewItems ?? Enumerable.Empty<EditID3File>())
            {
                Subscribe(file);
            }

            foreach (EditID3File file in (IEnumerable)e.OldItems ?? Enumerable.Empty<EditID3File>())
            {
                Unsubscribe(file);
            }

            foreach (EditID3File file in (IEnumerable)e.NewItems ?? Enumerable.Empty<EditID3File>())
            {
                if (value == defaultValue)
                {
                    if (Files != null && Files.Count > 1) continue;

                    value = GetValue(file);
                    IsDistinctValue = true;
                }
                else if (GetValue(file) != value)
                {
                    value = defaultValue;
                    IsDistinctValue = false;
                }
                else continue;

                OnPropertyChanged("Value");
                OnPropertyChanged("TitleStyle");
            }

            if (Files != null && Files.Count == 0)
            {
                value = defaultValue;
                IsDistinctValue = true;

                OnPropertyChanged("Value");
                OnPropertyChanged("TitleStyle");
            }
            else if (value == defaultValue && e.OldItems != null) Update();
        }

        private void GetValue(out string value, out bool isDistinctValue)
        {
            List<string> values = new List<string>();

            foreach (EditID3File file in Files ?? Enumerable.Empty<EditID3File>())
            {
                string tmp = GetValue(file);

                if (values.Contains(tmp)) continue;

                values.Add(tmp);

                if (values.Count < 2) continue;

                value = defaultValue;
                isDistinctValue = false;

                return;
            }

            if (values.Count == 0)
            {
                value = defaultValue;
                isDistinctValue = true;
            }
            else
            {
                value = values[0];
                isDistinctValue = false;
            }
        }

        protected abstract string GetValue(EditID3File file);

        protected abstract void SetValue(string value, EditID3File file);

        protected abstract void Subscribe(EditID3File file);

        protected abstract void Unsubscribe(EditID3File file);

        protected void Update()
        {
            string newValue;
            bool newIsDistinctValue;

            GetValue(out newValue, out newIsDistinctValue);

            value = newValue;
            IsDistinctValue = newIsDistinctValue;

            OnPropertyChanged("Value");
            OnPropertyChanged("TitleStyle");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
