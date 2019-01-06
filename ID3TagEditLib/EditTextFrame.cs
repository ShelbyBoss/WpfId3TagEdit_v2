using System;
using ID3TagLib;

namespace ID3TagEditLib
{
    public class EditTextFrame : EditFrame
    {
        public string Text
        {
            get { return (parent as TextFrame).Text; }
            set
            {
                if (value == (parent as TextFrame).Text) return;

                (parent as TextFrame).Text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        public EditTextFrame(TextFrame parent) : base(parent)
        {
            Text = parent.Text;
        }

        public static EditTextFrame GetFrameFromId(string frameId)
        {
            return new EditTextFrame((TextFrame)FrameFactory.GetFrame(frameId));
        }
    }
}
