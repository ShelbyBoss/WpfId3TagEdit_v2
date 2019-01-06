using ID3TagEditLib;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace WpfId3TagEdit
{
    public abstract class MultipleValueSyncronizer : ISync, INotifyPropertyChanged
    {
        private const string defaultValue = null;

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
                OnPropertyChanged(nameof(Files));
            }
        }

        public string Value
        {
            get { return value; }
            set
            {
                if (value == this.value) return;

                this.value = value;
                IsSync = true;

                foreach (EditID3File file in Files ?? Enumerable.Empty<EditID3File>())
                {
                    SetValue(value, file);
                    file.Save();
                }

                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(IsSync));
            }
        }

        public bool IsSync { get; private set; }

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
                    IsSync = GetIsSync(file);
                }
                else if (GetValue(file) != value)
                {
                    value = defaultValue;
                    IsSync = false;
                }
                else continue;

                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(IsSync));
            }

            if (Files != null && Files.Count == 0)
            {
                value = defaultValue;
                IsSync = true;

                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(IsSync));
            }
            else if (value == defaultValue && e.OldItems != null) Update();
        }

        private void GetValue(out string value, out bool isSync)
        {
            List<string> values = new List<string>();

            foreach (EditID3File file in Files ?? Enumerable.Empty<EditID3File>())
            {
                if (!GetIsSync(file))
                {
                    value = defaultValue;
                    isSync = false;
                }

                string tmp = GetValue(file);

                if (values.Contains(tmp)) continue;

                values.Add(tmp);

                if (values.Count < 2) continue;

                value = defaultValue;
                isSync = false;

                return;
            }

            if (values.Count == 0)
            {
                value = defaultValue;
                isSync = true;
            }
            else if (values.Count == 1)
            {
                value = values[0];
                isSync = true;
            }
            else
            {
                value = values[0];
                isSync = false;
            }
        }

        protected abstract string GetValue(EditID3File file);

        protected abstract bool GetIsSync(EditID3File file);

        protected abstract void SetValue(string value, EditID3File file);

        protected abstract void Subscribe(EditID3File file);

        protected abstract void Unsubscribe(EditID3File file);

        protected void Update()
        {
            string newValue;
            bool newIsDistinctValue;

            GetValue(out newValue, out newIsDistinctValue);

            value = newValue;
            IsSync = newIsDistinctValue;

            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(IsSync));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
