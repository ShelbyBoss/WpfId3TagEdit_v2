using ID3TagLib;
using System.Collections.Generic;
using System.Linq;

namespace ID3TagEditLib
{
    public static class EditFileExtenions
    {
        public static IEnumerable<EditTextFrame> GetIDv2TextFrames(this EditID3File file)
        {
            return file.V2Tag.OfType<EditTextFrame>();
        }

        public static void AddFrame(this EditID3File file, string id, string text)
        {
            TextFrame frame = FrameFactory.GetFrame(id) as TextFrame;
            frame.Text = text;

            file.V2Tag.Add(frame);
        }

        public static void RemoveFrame(this EditID3File file, string id, string text)
        {
            TextFrame frame = file.V2Tag.OfType<TextFrame>().FirstOrDefault(f => f.FrameId == id && f.Text == text);

            if (frame != null) file.V2Tag.Remove(frame);
        }

        public static void ChangeText(this EditID3File file, string id, string oldText, string newText)
        {
            TextFrame frame = file.V2Tag.OfType<TextFrame>().FirstOrDefault(f => f.FrameId == id && f.Text == oldText);

            if (frame != null) frame.Text = newText;
            else AddFrame(file, id, newText);
        }

        public static void ChangeId(this EditID3File file, string oldId, string newId, string text)
        {
            RemoveFrame(file, oldId, text);
            AddFrame(file, newId, text);
        }

        public static bool IsSyncronized(this EditID3File file)
        {
            return file.Artist.IsSync && file.Title.IsSync && file.Album.IsSync && 
                file.TrackNumber.IsSync && file.Year.IsSync && file.FileName.IsSync;
        }

        public static void RemoveNonTextFrames(this EditID3File file)
        {
            for (int i = file.V2Tag.Count - 1; i >= 0; i--)
            {
                if (!(file.V2Tag[i] is EditTextFrame)) file.V2Tag.RemoveAt(i);
            }
        }
    }
}
