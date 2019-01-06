using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ID3TagEditLib
{
    public class FileNameSynchronizer : ISync, INotifyPropertyChanged
    {
        private EditID3File source;
        private bool isSync;
        private string tagFileName, fileName;

        public EditID3File Source
        {
            get { return source; }
            private set
            {
                if (value == source) return;

                if (source != null)
                {
                    source.PropertyChanged -= Source_PropertyChanged;
                    source.Artist.PropertyChanged -= ArtistTitle_PropertyChanged;
                    source.Title.PropertyChanged -= ArtistTitle_PropertyChanged;
                }

                if (value != null)
                {
                    value.PropertyChanged += Source_PropertyChanged;
                    value.Artist.PropertyChanged += ArtistTitle_PropertyChanged;
                    value.Title.PropertyChanged += ArtistTitle_PropertyChanged;
                }

                source = value;
                OnPropertyChanged(nameof(Source));

                if (source != null)
                {
                    FileName = Path.GetFileNameWithoutExtension(source.Source.Name);
                    TagFileName = Source.Artist.Value + " - " + source.Title.Value;
                }
                else FileName = TagFileName = string.Empty;
            }
        }

        public bool IsSync  
        {
            get { return isSync; }
            private set
            {
                if (value == isSync) return;

                isSync = value;
                OnPropertyChanged(nameof(IsSync));
            }
        }

        public string TagFileName
        {
            get { return tagFileName; }
            private set
            {
                value = ToFileName(value);

                if (value == tagFileName) return;

                tagFileName = value;
                OnPropertyChanged(nameof(TagFileName));

                IsSync = TagFileName == FileName;
            }
        }

        public string FileName
        {
            get { return fileName; }
            set
            {
                value = ToFileName(value);

                if (value == fileName) return;

                fileName = value;
                OnPropertyChanged(nameof(FileName));

                IsSync = TagFileName == fileName;

                if (Source != null) Source.Source = ChangeFileName(Source.Source, fileName);
            }
        }

        //public FileNameSynchronizer()
        //{
        //    FileName = TagFileName = string.Empty;
        //    EqualsFileNameAndTag = true;
        //}

        public FileNameSynchronizer(EditID3File source)
        {
            Source = source;
        }

        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(source.Source)) FileName = Path.GetFileNameWithoutExtension(source.Source.Name);
        }

        private void ArtistTitle_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertySynchronizer synchronizer;

            if (e.PropertyName == nameof(synchronizer.Value)) TagFileName = Source.Artist.Value + " - " + source.Title.Value;
        }

        private string ToFileName(string fileName)
        {
            string newFileName = string.Empty;

            foreach (char c in fileName)
            {
                if (!Path.GetInvalidFileNameChars().Contains(c)) newFileName += c;
            }

            return newFileName;
        }

        private FileInfo ChangeFileName(FileInfo file, string newFileName)
        {
            string path = Path.Combine(file.DirectoryName, newFileName + file.Extension);

            return new FileInfo(path);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
