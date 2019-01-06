using ID3TagLib;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ID3TagEditLib
{
    public enum ArtistTitle { Artist, Title };

    public class EditID3File : INotifyPropertyChanged
    {
        private const string flieNameSeperator = " - ";

        private static readonly string[] blackIds = new string[] { "PRIV", "MCDI", "TCOP", "TLAN", "POPM", "TCOM",
                "TENC", "COMM", "NCON", "TPOS", "UFID", "TSSE", "WXXX", "IPLS", "WPUB", "TOWN", "TEXT", "TCMP",
                "TPE3", "TIPL", "TOFN", "WOAR", "WCOP", "GEOB", "TDAT", "TSOP", "RGAD", "TOPE", "TSIZ", "TIT1",
                "TPE4", "TSRC", "TIT3", "TKEY", "PCNT", "LINK", "TMED", "TRSN", "WOAF", "WOAS", "WORS", "WCOM",
                "TOLY", "TOLY", "TDES", "TGID", "TOAL", "TSOA", "TSO2", "TSOC", "TSOT", "WFED", "USER", "TRSO",
                "RVAD", "TIME" },
            whiteIds = new string[] { "APIC", "TIT2", "TDRC", "TALB", "TPE1", "TPE2", "TLEN", "TYER", "TFLT", "TRCK" },
            askIds = new string[] { "TCON", "TPUB", "TXXX", "USLT", "TDRL", "TBPM", "TCAT" };

        private ID3File parent;
        private FileInfo source;
        private EditID3v1Tag v1Tag;
        private EditID3v2Tag v2Tag;
        private PropertySynchronizer title, artist, album, trackNumber, year;
        private FileNameSynchronizer fileName;


        public FileInfo Source
        {
            get { return source; }
            set
            {
                if (value == null || value == source) return;

                if (source.FullName != value.FullName) source.MoveTo(value.FullName);

                source = value;
                OnPropertyChanged(nameof(Source));
            }
        }

        public EditID3v1Tag V1Tag
        {
            get { return v1Tag; }
            private set
            {
                if (value == v1Tag) return;

                v1Tag = value;
                parent.ID3v1Tag = value.Parent;

                OnPropertyChanged(nameof(V1Tag));
            }
        }

        public EditID3v2Tag V2Tag
        {
            get { return v2Tag; }
            private set
            {
                if (value == v2Tag) return;

                v2Tag = value;
                parent.ID3v2Tag = value.Parent;

                OnPropertyChanged(nameof(V2Tag));
            }
        }

        public PropertySynchronizer Title
        {
            get { return title; }
            private set
            {
                if (value == title) return;

                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public PropertySynchronizer Artist
        {
            get { return artist; }
            private set
            {
                if (value == artist) return;

                artist = value;
                OnPropertyChanged(nameof(Artist));
            }
        }

        public PropertySynchronizer Album
        {
            get { return album; }
            private set
            {
                if (value == album) return;

                album = value;
                OnPropertyChanged(nameof(Album));
            }
        }

        public PropertySynchronizer TrackNumber
        {
            get { return trackNumber; }
            private set
            {
                if (value == trackNumber) return;

                trackNumber = value;
                OnPropertyChanged(nameof(TrackNumber));
            }
        }

        public PropertySynchronizer Year
        {
            get { return year; }
            private set
            {
                if (value == year) return;

                year = value;
                OnPropertyChanged(nameof(Year));
            }
        }

        public FileNameSynchronizer FileName
        {
            get { return fileName; }
            private set
            {
                if (value == fileName) return;

                fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        public EditID3File(FileInfo source)
        {
            this.source = source;

            Reload();
        }

        public void Reload()
        {
            parent = new ID3File(source);

            if (parent.ID3v1Tag == null) parent.ID3v1Tag = new ID3v1Tag();
            if (parent.ID3v2Tag == null) parent.ID3v2Tag = new ID3v2Tag();

            V1Tag = new EditID3v1Tag(parent.ID3v1Tag);
            V2Tag = new EditID3v2Tag(parent.ID3v2Tag);

            Title = new PropertySynchronizer(V1Tag, V2Tag, nameof(V1Tag.Title), FrameFactory.TitleFrameId);
            Artist = new PropertySynchronizer(V1Tag, V2Tag, nameof(V1Tag.Artist), FrameFactory.LeadArtistFrameId, FrameFactory.BandFrameId);
            Album = new PropertySynchronizer(V1Tag, V2Tag, nameof(V1Tag.Album), FrameFactory.AlbumFrameId);
            TrackNumber = new PropertySynchronizer(V1Tag, V2Tag, nameof(V1Tag.TrackNumber), FrameFactory.TrackNumberFrameId);
            Year = new PropertySynchronizer(V1Tag, V2Tag, nameof(V1Tag.Year), FrameFactory.YearFrameId);

            FileName = new FileNameSynchronizer(this);
        }

        public void Save()
        {
            parent.Save(Source);
        }

        public void LoadID3(string file)
        {
            ID3File iD3File = new ID3File(file);
            iD3File.Save(Source);

            Reload();

            FileName.FileName = Path.GetFileNameWithoutExtension(file);
        }

        private bool TryGetTitleAndArtistBySource(out string title, out string artist)
        {
            title = null;
            artist = null;

            string fileName = GetFileName(source);
            int index = fileName.IndexOf(flieNameSeperator);

            if (index == -1) return false;

            title = fileName.Substring(index + flieNameSeperator.Length);
            artist = fileName.Remove(index);
            return true;
        }

        private string GetFileName(FileInfo source)
        {
            return Path.GetFileNameWithoutExtension(source.Name);
        }

        public void FileNameToID3Tag()
        {
            string title, artist;

            if (TryGetTitleAndArtistBySource(out title, out artist))
            {
                Title.Value = title;
                Artist.Value = artist;
            }
        }

        private string GetFileName(string artist, string title)
        {
            return string.Format("{0} - {1}", artist, title);
        }

        public void CompareID3v2TagWithPath(bool play, Func<ArtistTitle, string, string, string> askPathTag)
        {
            bool running = false;
            EditTextFrame frameArtist, frameTitle;
            string pathArtist, pathTitle;

            pathArtist = GetArtistFromFileInfo();
            pathTitle = GetSongTitleFromFileInfo();

            frameArtist = GetTextFrame(FrameFactory.LeadArtistFrameId, string.Empty);
            frameTitle = GetTextFrame(FrameFactory.TitleFrameId, string.Empty);

            if (pathArtist != frameArtist.Text)
            {
                if (play) Play();
                running = true;

                Artist.Value = askPathTag(ArtistTitle.Artist, pathArtist, frameArtist.Text);
            }

            if (pathTitle != frameTitle.Text)
            {
                if (!running && play) Play();

                Title.Value = askPathTag(ArtistTitle.Title, pathArtist, frameTitle.Text);
            }
        }

        public void AddTofnTag()
        {
            EditTextFrame artistFrame = GetTextFrame(FrameFactory.TitleFrameId);
            EditTextFrame titleFrame = GetTextFrame(FrameFactory.LeadArtistFrameId);

            if (artistFrame == null && titleFrame == null) return;

            string fileName;

            if (artistFrame == null) fileName = titleFrame.Text;
            else if (titleFrame == null) fileName = artistFrame.Text;
            else fileName = GetFileName(artistFrame.Text, titleFrame.Text);

            ChangeOrCreateTextFrame(FrameFactory.OriginalFilenameFrameId, fileName);
        }

        public void RemoveFrame(string id)
        {
            Frame frame;

            while (true)
            {
                frame = GetFrame(id);

                if (frame == null) break;

                V2Tag.Remove(frame);
            }
        }

        public void FilterID3v2Tags(Func<string, string, bool> askRemove)
        {
            for (int i = V2Tag.Count - 1; i >= 0; i--)
            {
                if (!IsFrameAllowed(V2Tag[i], askRemove)) V2Tag.RemoveAt(i);
            }
        }

        private void Play()
        {
            try
            {
                Source.CopyTo("tmp1.mp3");
                Process.Start("tmp1.mp3");
                return;
            }
            catch { }

            try
            {
                Source.CopyTo("tmp2.mp3");
                Process.Start("tmp2.mp3");
            }
            catch { }
        }

        private EditFrame GetFrame(string id)
        {
            return V2Tag.FirstOrDefault(f => f.FrameId == id);
        }

        private EditTextFrame GetTextFrame(string id)
        {
            return V2Tag.OfType<EditTextFrame>().FirstOrDefault(f => f.FrameId == id);
        }

        private EditTextFrame GetTextFrame(string id, string defaultValue)
        {
            return GetTextFrame(id) ?? CreateTextFrame(id, defaultValue);
        }

        public void ChangeOrCreateTextFrame(string id, string value)
        {
            EditTextFrame frame = GetTextFrame(id) ?? (EditTextFrame)FrameFactory.GetFrame(id);

            frame.Text = value;
        }

        private EditTextFrame CreateTextFrame(string id, string value)
        {
            EditTextFrame frame = (EditTextFrame)FrameFactory.GetFrame(id);
            frame.Text = value;

            V2Tag.Add(frame);

            return frame;
        }

        private string GetSongTitleFromFileInfo()
        {
            char[] c = GetFileNameWithoutExtension(Source).ToCharArray();

            int index = GetBindestrichIndex(c);

            try
            {
                if (index != -1) return GetFileNameWithoutExtension(Source).Remove(0, index + 2);
            }
            catch { }

            return GetFileNameWithoutExtension(Source);
        }

        private string GetArtistFromFileInfo()
        {
            char[] c = GetFileNameWithoutExtension(Source).ToCharArray();

            int index = GetBindestrichIndex(c);

            try
            {
                if (index != -1) return GetFileNameWithoutExtension(Source).Remove(index - 1);
            }
            catch { }

            return GetFileNameWithoutExtension(Source);
        }

        private bool IsFrameAllowed(EditFrame frame, Func<string, string, bool> askRemove)
        {
            if (!(frame is EditTextFrame)) return false;

            string id = frame.FrameId, text = (frame as EditTextFrame).Text;
            if (text == string.Empty) return false;

            if (blackIds.Any(i => i == id)) return false;
            if (whiteIds.Any(i => i == id)) return false;

            if (askIds.Any(i => i == id && askRemove(id, text))) return false;

            return true;
        }

        private FileInfo GetChangedFileInfo()
        {
            bool withBrackets = false;
            char[] c = GetFileNameWithoutExtension(Source).ToCharArray();
            int featStart = -1, featLength = 0, featBrackets = -1, artistEnd = GetBindestrichIndex(c);

            for (int i = artistEnd; i < c.Length; i++, featLength++)
            {
                if (artistEnd != -1 && i + 5 < c.Length && c[i] == 'f' && c[i + 1] == 'e' &&
                    c[i + 2] == 'a' && c[i + 3] == 't' && c[i + 4] == '.' && c[i + 5] == ' ')
                {
                    featStart = i;
                    featLength = 0;

                    featBrackets = i > 0 && c[i - 1] == '(' ? 1 : -1;
                }
                else if (featBrackets >= 0 && c[i] == '(')
                {
                    featBrackets++;
                }
                else if (featBrackets >= 0 && c[i] == ')')
                {
                    featBrackets--;
                    withBrackets = featBrackets == 0;
                    break;
                }
            }

            string filename = GetStringFromCharArrayWithStartAndLength(c, 0, artistEnd);

            if (featStart >= 0)
            {
                filename += GetStringFromCharArrayWithStartAndLength(c, featStart, featLength) + " ";

                if (withBrackets)
                {
                    featStart--;
                    featLength += 2;
                }

                filename += GetStringFromCharArrayWithStartAndLength(c, artistEnd, featStart - artistEnd).TrimEnd(' ');
                filename += GetStringFromCharArrayWithStartAndLength(c, featStart + featLength, c.Length - featStart - featLength);
            }
            else
            {
                filename = artistEnd != -1 ? filename + GetStringFromCharArrayWithStartAndLength(c, artistEnd, c.Length - artistEnd) :
                    GetFileNameWithoutExtension(Source);
            }

            return new FileInfo(Source.DirectoryName + "\\" + filename + Source.Extension);
        }

        private string GetStringFromCharArrayWithStartAndLength(char[] array, int start, int length)
        {
            string text = string.Empty;

            for (int i = 0; i < length; i++)
            {
                text += array[start + i];
            }

            return text;
        }

        private int GetBindestrichIndex(char[] c)
        {
            for (int i = 0; i < c.Length; i++)
            {
                if (i + 2 < c.Length && c[i] == ' ' && c[i + 1] == '-' && c[i + 2] == ' ')
                {
                    return i + 1;
                }
            }

            return -1;
        }

        private string GetFileNameWithoutExtension(FileInfo fileInfo)
        {
            return fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
